using System.ComponentModel.DataAnnotations;

namespace ASE.Libraries.Search;

public class SearchOptions
{
    public const string SectionName = "Search";

    [Required]
    public string Environment { get; set; } = string.Empty;

    public string AzureSearchEndpoint { get; set; } = string.Empty;
    public string AzureSearchIndexName { get; set; } = string.Empty;
    public string AzureSearchSemanticConfig { get; set; } = string.Empty;
    public string KnowledgeBaseName { get; set; } = string.Empty;
}
