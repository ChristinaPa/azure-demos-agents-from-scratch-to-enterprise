using System.Diagnostics;
using System.Text.Json;
using ASE.Libraries.Models;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ASE.Libraries.Search;

public class AzureSearchDocumentSearchAdapter : ISearchService
{
    private readonly SearchClient _client;
    private readonly string _semanticConfigName;
    private readonly string _searchEndpoint;
    private readonly string _knowledgeBaseName;

    public AzureSearchDocumentSearchAdapter(
        SearchClient client,
        string semanticConfigName = "",
        string searchEndpoint = "",
        string knowledgeBaseName = "")
    {
        _client = client;
        _semanticConfigName = semanticConfigName;
        _searchEndpoint = searchEndpoint;
        _knowledgeBaseName = knowledgeBaseName;
    }

    public List<SearchResult> Search(string query, int records = 10)
    {
        var options = new Azure.Search.Documents.SearchOptions
        {
            Size = records,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _semanticConfigName
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

        var response = _client.Search<SearchDocument>(query, options);

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
        ArgumentException.ThrowIfNullOrEmpty(_searchEndpoint, "Search:AzureSearchEndpoint configuration is not set.");
        ArgumentException.ThrowIfNullOrEmpty(_knowledgeBaseName, "Search:KnowledgeBaseName configuration is not set.");
        var translationLanguage = Environment.GetEnvironmentVariable("Language");
        ArgumentException.ThrowIfNullOrEmpty(translationLanguage, "Language environment variable is not set.");

        #endregion

        var credential = new DefaultAzureCredential();

        // Acquire bearer token for the Azure AI Search knowledge-base MCP endpoint.
        var tokenContext = new TokenRequestContext(new[] { "https://search.azure.com/.default" });
        var token = await credential.GetTokenAsync(tokenContext, CancellationToken.None);

        // KB MCP endpoint: https://<svc>.search.windows.net/knowledgebases/<kb>/mcp?api-version=2025-11-01-preview
        var mcpEndpoint = new Uri(
            $"{_searchEndpoint.TrimEnd('/')}/knowledgebases/{_knowledgeBaseName}/mcp?api-version=2025-11-01-preview");

        // Chat client used by both the search agent (for tool calling) and the translation agent.
        IChatClient client =
            new ChatClientBuilder(
                    new AzureOpenAIClient(new Uri(endpoint), credential)
                        .GetChatClient(deploymentName)
                        .AsIChatClient())
                .Build();

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "Azure AI Search Knowledge Base",
                Endpoint = mcpEndpoint,
                AdditionalHeaders = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {token.Token}"
                }
            });

        await using var mcpClient = await McpClient.CreateAsync(transport);
        var tools = await mcpClient.ListToolsAsync();

        // Search agent: calls the KB's knowledge_base_retrieve MCP tool.
        var searchAgent = client.AsAIAgent(
            instructions:
            "You are a friendly assistant. Use the knowledge_base_retrieve tool to ground your answer " +
            "in the Azure AI Search knowledge base. Pass the user's question through unchanged. " +
            "Return the tool result verbatim so a downstream agent can post-process it.",
            name: "AgenticRetrievalAgent",
            tools: [..tools]);

        // Translation + reshape agent: translates the grounding content and emits SearchResult JSON.
        var translationAgent = new ChatClientAgent(client,
            $"You are a translation assistant who responds in {translationLanguage}. " +
            "The previous agent returned grounded search results from an Azure AI Search knowledge base " +
            "(an array of objects with fields such as ref_id, title, terms, content). " +
            $"For each item, translate the 'content' (or equivalent text) field into {translationLanguage}, " +
            "and output ONLY a JSON array of objects with exactly these properties: " +
            "\"sourceName\" (use 'title' if present, otherwise 'ref_id'), " +
            "\"sourceLink\" (use 'ref_id' or document key), " +
            "\"text\" (the translated content). " +
            "Do not wrap the JSON in markdown fences. Do not add commentary.");

        var workflow = AgentWorkflowBuilder.BuildSequential(searchAgent, translationAgent);
        var messages = new List<ChatMessage> { new(ChatRole.User, query) };

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