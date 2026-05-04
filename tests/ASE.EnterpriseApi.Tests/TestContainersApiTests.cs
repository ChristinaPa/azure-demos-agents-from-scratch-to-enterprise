using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace ASE.EnterpriseApi.Tests;

/// <summary>
/// Integration tests that build and run the ASE.EnterpriseApi Docker image using Testcontainers.
/// Requires Docker Desktop (or compatible daemon) to be running.
/// Run with:  dotnet test --filter "Category=Integration"
/// Skip with: dotnet test --filter "Category!=Integration"
/// </summary>
[Trait("Category", "Integration")]
public class TestContainersApiTests : IAsyncLifetime
{
    private IFutureDockerImage? _image;
    private IContainer? _container;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task InitializeAsync()
    {
        // Build context: src/AgentScratchEnterprise/
        // Dockerfile:    ASE.EnterpriseApi/Dockerfile  (relative to build context)
        _image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "src/AgentScratchEnterprise")
            .WithDockerfile("ASE.EnterpriseApi/Dockerfile")
            .WithBuildArgument("BUILD_CONFIGURATION", "Release")
            .Build();

        await _image.CreateAsync();

        _container = new ContainerBuilder()
            .WithImage(_image)
            .WithPortBinding(80, assignRandomHostPort: true)
            .WithEnvironment("Search__Environment", "LOCAL")
            .WithEnvironment("Cors__AllowedOrigins__0", "http://localhost:5173")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/health")))
            .Build();

        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();

        if (_image is not null)
            await _image.DisposeAsync();
    }

    [Fact]
    public async Task Search_Return_ReturnsNonEmptyResults_ViaDockerContainer()
    {
        Assert.NotNull(_container);

        var hostPort = _container.GetMappedPublicPort(80);
        using var client = new HttpClient();
        var url = $"http://localhost:{hostPort}/basic/search?query=return";

        var response = await client.GetAsync(url);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<SearchResultDto>>(json, JsonOptions);

        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.SourceName.Contains("Contoso Outdoors Return Policy", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record SearchResultDto(string SourceName, string SourceLink, string Text);
}
