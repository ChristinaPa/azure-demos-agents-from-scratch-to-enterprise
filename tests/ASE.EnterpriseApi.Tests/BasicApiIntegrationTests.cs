using System.Text.Json;
using ASE.EnterpriseApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ASE.EnterpriseApi.Tests;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Search:Environment"] = "LOCAL",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
            });
        });
        builder.UseEnvironment("Test");
    }
}

public class BasicApiIntegrationTests(ApiWebApplicationFactory factory)
    : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAll_Returns200_WithNonEmptyBankDataArray()
    {
        var response = await _client.GetAsync("/basic/get-all");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement[]>(json, JsonOptions);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task Search_Return_Returns200_WithContosoReturnPolicy()
    {
        var response = await _client.GetAsync("/basic/search?query=return");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<SearchResultDto>>(json, JsonOptions);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.SourceName.Contains("Contoso Outdoors Return Policy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_Refund_Returns200_WithNonEmptyResults()
    {
        var response = await _client.GetAsync("/basic/search?query=refund");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<SearchResultDto>>(json, JsonOptions);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task Search_Amount_Returns200_WithTransactionResults()
    {
        var response = await _client.GetAsync("/basic/search?query=amount");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<SearchResultDto>>(json, JsonOptions);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Text.Contains("transaction", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_UnrelatedQuery_Returns200_WithEmptyArray()
    {
        var response = await _client.GetAsync("/basic/search?query=somethingcompletelyunrelated");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<SearchResultDto>>(json, JsonOptions);

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task Health_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record SearchResultDto(string SourceName, string SourceLink, string Text);
}
