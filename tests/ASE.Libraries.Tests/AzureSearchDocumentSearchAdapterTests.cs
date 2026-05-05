using ASE.Libraries;
using ASE.Libraries.Search;
using Microsoft.Extensions.Options;

namespace ASE.Libraries.Tests;

public class AzureSearchDocumentSearchAdapterTests
{
    private readonly AzureSearchDocumentSearchAdapter _adapter;
    private static readonly IOptions<SearchOptions> FakeOptions = Options.Create(new SearchOptions
    {
        Environment = "AZURE",
        AzureSearchEndpoint = "https://fake.search.windows.net",
        AzureSearchIndexName = "fake-index",
        AzureSearchSemanticConfig = "fake-semantic",
        KnowledgeBaseName = "fake-kb"
    });

    public AzureSearchDocumentSearchAdapterTests()
    {
        _adapter = new AzureSearchDocumentSearchAdapter(FakeOptions);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_ImplementsISearchService()
    {
        // Assert
        Assert.IsAssignableFrom<ISearchService>(_adapter);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_RequiresOptions()
    {
        // Assert - constructor requires IOptions<SearchOptions>
        Assert.NotNull(_adapter);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_CanBeInstantiated()
    {
        // Act
        var adapter = new AzureSearchDocumentSearchAdapter(FakeOptions);

        // Assert
        Assert.NotNull(adapter);
    }
}
