using System.ComponentModel;
using ASE.Libraries.Models;
using ASE.Libraries.Search;
using ModelContextProtocol.Server;

namespace ASE.EnterpriseApi.Tools;

[McpServerToolType]
public class BasicSearchTool(ILogger<BasicSearchTool> logger,
    ISearchService searchService)
{
    [McpServerTool(Name = "search_api",
        Title = "Search API for customer data")]
    [Description("Search API for customer data and return policies")]
    public List<SearchResult> SearchingBasedOnConfiguration(string query)
    {
        logger.LogInformation("Received search query: {Query}", query);
        var data = searchService.Search(query);
        logger.LogInformation("Search returned {Count} results", data.Count);
        return data;
    }
}