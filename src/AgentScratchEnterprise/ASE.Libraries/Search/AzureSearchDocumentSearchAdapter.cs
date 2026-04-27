using ASE.Libraries.Models;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace ASE.Libraries.Search;

public class AzureSearchDocumentSearchAdapter : ISearchService
{
    private readonly SearchClient _client;
    private readonly string _semanticConfigName;

    public AzureSearchDocumentSearchAdapter(SearchClient client, string semanticConfigName = "")
    {
        _client = client;
        _semanticConfigName = semanticConfigName;
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
        var options = new Azure.Search.Documents.SearchOptions
        {
            Size = records,
            QueryType = SearchQueryType.Semantic,
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = _semanticConfigName
            }
        };
        options.Select.Add("chunk_id");
        options.Select.Add("title");
        options.Select.Add("chunk");
        options.HighlightFields.Add("chunk");

        var response = await _client.SearchAsync<SearchDocument>(query, options);

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
}