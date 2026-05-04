using System.Diagnostics;
using System.Text.Json;
using ASE.Libraries.Data;
using ASE.Libraries.Models;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ASE.Libraries.Search;

/// <summary>
/// Provides a mock document-search back-end for the RAG text-search sample.
/// In production this would call an actual search index (e.g. Azure AI Search).
/// </summary>
public class DocumentSearchAdapter : ISearchService
{
    /// <summary>
    /// Returns matching document snippets for the supplied query.
    /// Currently, supports return/refund policy lookups and also bank record check.
    /// </summary>
    /// <returns>
    ///     A list of <see cref="SearchResult"/> objects matching the query.
    /// </returns>
    public List<SearchResult> Search(string query, int records = 10)
    {
        var list = new List<SearchResult>();
        //search policies we have as company
        if (query.Contains("return", StringComparison.OrdinalIgnoreCase) ||
            query.Contains("refund", StringComparison.OrdinalIgnoreCase))
        {
            list.Add(new SearchResult
            {
                SourceName = "Contoso Outdoors Return Policy",
                SourceLink = "https://contoso.com/policies/returns",
                Text = "Customers may return any item within 30 days of delivery. " +
                       "Items should be unused and include original packaging. " +
                       "Refunds are issued to the original payment method within 5 business days of inspection."
            });
        }

        //if we are searching for an amount, get all transactions from bank, which are bigger than 1000
        if (query.Contains("amount", StringComparison.OrdinalIgnoreCase))
        {
            var dataWithStatements = BankDataGenerator.GenerateBankDataWithStatementList(records);
            var transactionsAbove1k = dataWithStatements.MaxBy(currentStatement =>
                currentStatement.Statement.Transactions.MaxBy(currentTransaction => currentTransaction.Amount)?.Amount >
                1000);
            foreach (var transaction in transactionsAbove1k.Statement.Transactions)
            {
                list.Add(new SearchResult
                {
                    SourceName = transaction.BankCard.CardHolderName,
                    SourceLink =
                        $"https://contoso.com/accounts/{transaction.BankCard.CardNumber}/statements/{transaction.BankCard.CardNumber}",
                    Text =
                        $"Account {transaction.BankCard.CardNumber} has a transaction of amount {transaction.Amount} on {transaction.Date:d} with description '{transaction.Description}'"
                });
            }
        } //after data will be returned, LLM will do the work based on instructions

        return list;
    }

    public async Task<List<SearchResult>> AdvancedSearch(string query, int records = 10)
    {
        #region Environment variables

        var endpoint = Environment.GetEnvironmentVariable("AzureFoundryEndpoint");
        ArgumentException.ThrowIfNullOrEmpty(endpoint, "Endpoint environment variable is not set.");
        var deploymentName = Environment.GetEnvironmentVariable("DeploymentName");
        ArgumentException.ThrowIfNullOrEmpty(deploymentName, "DeploymentName environment variable is not set.");
        var mcpEndpoint = Environment.GetEnvironmentVariable("McpEndpoint");
        ArgumentException.ThrowIfNullOrEmpty(mcpEndpoint, "McpEndpoint environment variable is not set.");
        var translationLanguage = Environment.GetEnvironmentVariable("Language");
        ArgumentException.ThrowIfNullOrEmpty(translationLanguage, "Language environment variable is not set.");

        #endregion

        var credentials = new DefaultAzureCredential();
        IChatClient client =
            new ChatClientBuilder(
                    new AzureOpenAIClient(new Uri(endpoint), credentials)
                        .GetChatClient(deploymentName)
                        .AsIChatClient())
                .Build();
        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Name = "My Business Api Search",
                Endpoint = new Uri(mcpEndpoint)
            });
        var mcpClient = await McpClient.CreateAsync(transport);
        var tools = await mcpClient.ListToolsAsync();
        //agent with tools
        var searchClient = client.AsAIAgent(
            instructions: "You are a friendly assistant. Keep your answers brief.",
            name: "SimpleAgentWithMCP",
            tools: [..tools]);
        //translation agent
        var translationAgent = new ChatClientAgent(client,
            $"You are a translation assistant who responds in {translationLanguage}." +
            "Take tool results, translate text property and replace text property with translation." +
            "Keep source name and source link the same and return that to calling function.");

        //build sequential workflow
        var workflow = AgentWorkflowBuilder.BuildSequential(searchClient, translationAgent);
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
        
        var list  = new List<SearchResult>();
        foreach (var message in result)
        {
            if (message.Role == ChatRole.Assistant)
            {
                Debug.WriteLine($"[blue]{message.Role}[/]: [red]{message.Text}[/]");
                TryAddSourceEntry(message.Text, list);
            }
        }

        return list;
    }

    private static bool TryAddSourceEntry(
        string input,
        List<SearchResult> list)
    {
        try
        {
            var entry = JsonSerializer.Deserialize<List<SearchResult>>(
                input,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (entry != null && IsValid(entry))
            {
                list.AddRange(entry);
                return true;
            }
        }
        catch (JsonException err)
        {
            // Invalid JSON → skip
            Debug.WriteLine(err);
        }

        return false;
    }

    private static bool IsValid(List<SearchResult> entry)
    {
        if (entry == null || entry.Count == 0)
        {
            return false;
        }
        return true;
    }

}

