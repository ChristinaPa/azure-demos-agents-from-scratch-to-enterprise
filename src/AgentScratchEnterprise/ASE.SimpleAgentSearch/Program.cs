using ASE.Libraries;
using ASE.Libraries.Search;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;

#region Environment variables

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT");
ArgumentException.ThrowIfNullOrEmpty(endpoint, "Please set the ENDPOINT environment variable.");
var deploymentName = Environment.GetEnvironmentVariable("DEPLOYMENTNAME");
ArgumentException.ThrowIfNullOrEmpty(deploymentName, "Please set the DEPLOYMENTNAME environment variable.");
var applicationInsightsConnectionString = Environment.GetEnvironmentVariable("APPLICATION_INSIGHTS_CONNECTION_STRING");
ArgumentException.ThrowIfNullOrEmpty(applicationInsightsConnectionString, "applicationInsightsConnectionString environment variable is not set.");

#endregion

#region Configure OpenTelemetry

const string SourceName = "AppInsightsWithAgents";
const string ServiceName = "AgentOpenTelemetry";

var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService(ServiceName);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource(SourceName)
    .AddAzureMonitorTraceExporter(options => options.ConnectionString = applicationInsightsConnectionString)
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddAzureMonitorMetricExporter(options => options.ConnectionString = applicationInsightsConnectionString)
    .Build();

#endregion

TextSearchProviderOptions textSearchOptions = new()
{
    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
};

IChatClient client =
    new ChatClientBuilder(
            new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
                .GetChatClient(deploymentName)
                .AsIChatClient())
        .UseOpenTelemetry(
            sourceName: "AgentWithSearch",
            configure: cfg =>
                cfg.EnableSensitiveData = true)
        .Build();

AIAgent agent = client
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            Instructions = "You are a helpful support specialist. Answer questions using the provided context and cite the source document when available."
        },
        AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)] 
    });
var question = AnsiConsole.Ask<string>("Ask your question",
    "What is the return policy?");
AnsiConsole.MarkupLine("[green]Question:[/] " + question);
var agentResponse = await agent.RunAsync(question);
AnsiConsole.MarkupLine("[green]Answer:[/] " + agentResponse.Text);

static Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string query, CancellationToken cancellationToken)
{
    var data = new DocumentSearchAdapter();
    var results = data.Search(query)
        .Select(r => new TextSearchProvider.TextSearchResult
        {
            SourceName = r.SourceName,
            SourceLink = r.SourceLink,
            Text = r.Text
        });
    return Task.FromResult(results);
}