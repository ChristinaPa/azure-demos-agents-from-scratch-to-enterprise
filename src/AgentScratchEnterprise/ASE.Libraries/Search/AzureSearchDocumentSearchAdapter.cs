using System.Diagnostics;
using System.Text.Json;
using ASE.Libraries.Models;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;
using Azure.Search.Documents.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace ASE.Libraries.Search;

/// <summary>
/// Azure AI Search adapter. All dependencies (SearchClient, knowledge-base
/// retrieval client, configuration) are constructed inside this class so a
/// developer reading it sees the full picture in one place. Authentication
/// is keyless via <see cref="DefaultAzureCredential"/> (SFI / managed identity).
/// </summary>
public class AzureSearchDocumentSearchAdapter : ISearchService
{
    private readonly SearchOptions _options;
    private readonly SearchClient _searchClient;
    private readonly DefaultAzureCredential _credential;

    public AzureSearchDocumentSearchAdapter(IOptions<SearchOptions> options)
    {
        _options = options.Value;
        _credential = new DefaultAzureCredential();

        _searchClient = new SearchClient(
            new Uri(_options.AzureSearchEndpoint),
            _options.AzureSearchIndexName,
            _credential);
    }

    public List<SearchResult> Search(string query, int records = 10)
    {
        var options = new Azure.Search.Documents.SearchOptions
        {
            Size = records,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _options.AzureSearchSemanticConfig
            },
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizableTextQuery(query)
                    {
                        KNearestNeighborsCount = records,
                        Fields = { "text_vector" }
                    }
                }
            }
        };
        options.SearchFields.Add("chunk");
        options.SearchFields.Add("title");
        options.Select.Add("chunk_id");
        options.Select.Add("title");
        options.Select.Add("chunk");

        var response = _searchClient.Search<SearchDocument>(query, options);

        var scored = response.Value.GetResults().ToList();
        var topRerankerScore = scored.FirstOrDefault()?.SemanticSearch.RerankerScore ?? 0;
        var threshold = topRerankerScore * 0.6;

        return scored
            .Where(r => r.SemanticSearch.RerankerScore >= threshold)
            .Take(5)
            .Select(r => new SearchResult
            {
                SourceName = r.Document.GetString("title"),
                SourceLink = r.Document.GetString("chunk_id"),
                Text       = r.Document.GetString("chunk")
            }).ToList();
    }

    public async Task<List<SearchResult>> AdvancedSearch(string query, int records = 10)
    {
        #region Environment variables

        var endpoint = Environment.GetEnvironmentVariable("AzureFoundryEndpoint");
        ArgumentException.ThrowIfNullOrEmpty(endpoint, "AzureFoundryEndpoint environment variable is not set.");
        var deploymentName = Environment.GetEnvironmentVariable("DeploymentName");
        ArgumentException.ThrowIfNullOrEmpty(deploymentName, "DeploymentName environment variable is not set.");
        ArgumentException.ThrowIfNullOrEmpty(_options.AzureSearchEndpoint, "Search:AzureSearchEndpoint configuration is not set.");
        ArgumentException.ThrowIfNullOrEmpty(_options.KnowledgeBaseName, "Search:KnowledgeBaseName configuration is not set.");
        var translationLanguage = Environment.GetEnvironmentVariable("Language");
        ArgumentException.ThrowIfNullOrEmpty(translationLanguage, "Language environment variable is not set.");

        #endregion

        // 1. Call the Azure AI Search knowledge-base retrieve action to ground the answer.
        //    Docs: https://learn.microsoft.com/en-us/azure/search/agentic-retrieval-how-to-retrieve?pivots=csharp#call-the-retrieve-action
        var kbClient = new KnowledgeBaseRetrievalClient(
            endpoint: new Uri(_options.AzureSearchEndpoint),
            knowledgeBaseName: _options.KnowledgeBaseName,
            tokenCredential: _credential);

        var retrievalRequest = new KnowledgeBaseRetrievalRequest();
        retrievalRequest.Messages.Add(
            new KnowledgeBaseMessage(
                content: new[] { new KnowledgeBaseMessageTextContent(query) })
            {
                Role = "user"
            });

        var retrievalResponse = await kbClient.RetrieveAsync(retrievalRequest);
        var groundedText =
            (retrievalResponse.Value.Response[0].Content[0] as KnowledgeBaseMessageTextContent)?.Text
            ?? string.Empty;

        // 2. Translate + reshape the grounded JSON into SearchResult JSON via a chat agent.
        IChatClient chatClient =
            new ChatClientBuilder(
                    new AzureOpenAIClient(new Uri(endpoint), _credential)
                        .GetChatClient(deploymentName)
                        .AsIChatClient())
                .Build();

        var translationAgent = new ChatClientAgent(chatClient,
            $"You are a translation assistant who responds in {translationLanguage}. " +
            "The user message contains grounded search results from an Azure AI Search knowledge base " +
            "(an array of objects with fields such as ref_id, title, terms, content). " +
            $"For each item, translate the 'content' (or equivalent text) field into {translationLanguage}, " +
            "and output ONLY a JSON array of objects with exactly these properties: " +
            "\"sourceName\" (use 'title' if present, otherwise 'ref_id'), " +
            "\"sourceLink\" (use 'ref_id' or document key), " +
            "\"text\" (the translated content). " +
            "Do not wrap the JSON in markdown fences. Do not add commentary.");

        var workflow = AgentWorkflowBuilder.BuildSequential(translationAgent);
        var messages = new List<ChatMessage> { new(ChatRole.User, groundedText) };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: false));

        List<ChatMessage> result = [];
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>()!;
                break;
            }
        }

        var list = new List<SearchResult>();
        foreach (var message in result)
        {
            if (message.Role == ChatRole.Assistant)
            {
                Debug.WriteLine($"{message.Role}: {message.Text}");
                TryAddSourceEntry(message.Text, list);
            }
        }

        return list;
    }

    private static bool TryAddSourceEntry(string input, List<SearchResult> list)
    {
        try
        {
            var entry = JsonSerializer.Deserialize<List<SearchResult>>(
                input,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (entry is { Count: > 0 })
            {
                list.AddRange(entry);
                return true;
            }
        }
        catch (JsonException)
        {
            // Invalid JSON → skip
        }

        return false;
    }
}
