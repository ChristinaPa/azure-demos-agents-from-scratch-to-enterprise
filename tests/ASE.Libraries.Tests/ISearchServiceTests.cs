using ASE.Libraries;
using ASE.Libraries.Search;
using Azure;
using Azure.Search.Documents;

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
        var client = new SearchClient(
            new Uri("https://fake.search.windows.net"),
            "fake-index",
            new AzureKeyCredential("fake-key"));
        var adapter = new AzureSearchDocumentSearchAdapter(client);

        // Assert
        Assert.IsAssignableFrom<ISearchService>(adapter);
    }
}
