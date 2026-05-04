using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using ModelContextProtocol.Client;
using Spectre.Console;

AnsiConsole.MarkupLine("[blue]Tools Agent Example: Call with MCP[/]");

#region ENV variables

var endpoint = Environment.GetEnvironmentVariable("ENDPOINT");
ArgumentException.ThrowIfNullOrEmpty(endpoint, "ENDPOINT environment variable is not set.");
var deploymentName = Environment.GetEnvironmentVariable("DEPLOYMENTNAME");
ArgumentException.ThrowIfNullOrEmpty(deploymentName, "DEPLOYMENTNAME environment variable is not set.");
var mcpEndpoint = Environment.GetEnvironmentVariable("McpEndpoint");
ArgumentException.ThrowIfNullOrEmpty(mcpEndpoint, "McpEndpoint environment variable is not set.");

AnsiConsole.MarkupLine("[blue]Endpoint [/]: " + endpoint);
AnsiConsole.MarkupLine("[blue]Deployment Name[/]: " + deploymentName);
AnsiConsole.MarkupLine("[blue]Mcp Endpoint[/]: " + mcpEndpoint);

#endregion

var transport = new HttpClientTransport(
    new HttpClientTransportOptions
    {
        Name = "My Business Api Search",
        Endpoint = new Uri(mcpEndpoint)
    });
var mcpClient = await McpClient.CreateAsync(transport);
var tools = await mcpClient.ListToolsAsync();
AnsiConsole.MarkupLine("[blue]Tools available:[/]");
foreach (var mcpClientTool in tools)
{
    AnsiConsole.WriteLine($"{mcpClientTool.Name} - {mcpClientTool.Description}");
}

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: "You are a friendly assistant. Keep your answers brief.",
        name: "SimpleAgentWithMCP",
        tools:[..tools]);
var question = AnsiConsole.Ask<string>("Ask your question",
    "What is the return policy for Contoso?");
AnsiConsole.MarkupLine("[green]Question:[/]" + question);
var answer = await agent.RunAsync(question);
AnsiConsole.MarkupLine("[green]Answer:[/]" + answer);