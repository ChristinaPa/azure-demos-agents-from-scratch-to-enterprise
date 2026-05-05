using ASE.Libraries;
using ASE.Libraries.Search;
using Microsoft.Extensions.Options;

namespace ASE.Libraries.Tests;

public class ISearchServiceTests
{
    [Fact]
    public void ISearchService_IsAnInterface()
    {
        // Assert
        Assert.True(typeof(ISearchService).IsInterface);
    }

    [Fact]
    public void ISearchService_HasSearchMethod()
    {
        // Act
        var method = typeof(ISearchService).GetMethod("Search");

        // Assert
        Assert.NotNull(method);
        Assert.Equal("Search", method.Name);
    }

    [Fact]
    public void DocumentSearchAdapter_ImplementsISearchService()
    {
        // Act
        var adapter = new DocumentSearchAdapter();

        // Assert
        Assert.IsAssignableFrom<ISearchService>(adapter);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_ImplementsISearchService()
    {
        // Act
        var options = Options.Create(new SearchOptions
        {
            Environment = "AZURE",
            AzureSearchEndpoint = "https://fake.search.windows.net",
            AzureSearchIndexName = "fake-index",
            AzureSearchSemanticConfig = "fake-semantic",
            KnowledgeBaseName = "fake-kb"
        });
        var adapter = new AzureSearchDocumentSearchAdapter(options);

        // Assert
        Assert.IsAssignableFrom<ISearchService>(adapter);
    }
}
