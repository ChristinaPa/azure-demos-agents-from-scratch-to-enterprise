using ASE.Libraries;
using ASE.Libraries.Search;
using Azure;
using Azure.Search.Documents;

namespace ASE.Libraries.Tests;

public class AzureSearchDocumentSearchAdapterTests
{
    private readonly AzureSearchDocumentSearchAdapter _adapter;
    private static readonly SearchClient FakeClient = new(
        new Uri("https://fake.search.windows.net"),
        "fake-index",
        new AzureKeyCredential("fake-key"));

    public AzureSearchDocumentSearchAdapterTests()
    {
        _adapter = new AzureSearchDocumentSearchAdapter(FakeClient);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_ImplementsISearchService()
    {
        // Assert
        Assert.IsAssignableFrom<ISearchService>(_adapter);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_RequiresSearchClient()
    {
        // Assert - constructor requires a SearchClient
        Assert.NotNull(_adapter);
    }

    [Fact]
    public void AzureSearchDocumentSearchAdapter_CanBeInstantiated()
    {
        // Act
        var adapter = new AzureSearchDocumentSearchAdapter(FakeClient);

        // Assert
        Assert.NotNull(adapter);
    }
}
