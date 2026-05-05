using System.ComponentModel.DataAnnotations;
using ASE.Libraries.Search;
using Microsoft.Extensions.Configuration;

namespace ASE.EnterpriseApi.Tests;

public class SearchOptionsTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        Assert.Equal("Search", SearchOptions.SectionName);
    }

    [Fact]
    public void Environment_Required_FailsValidationWhenNull()
    {
        var options = new SearchOptions { Environment = null! };
        var results = ValidateOptions(options);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SearchOptions.Environment)));
    }

    [Fact]
    public void Environment_Required_FailsValidationWhenEmpty()
    {
        var options = new SearchOptions { Environment = "" };
        var results = ValidateOptions(options);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(SearchOptions.Environment)));
    }

    [Fact]
    public void Environment_PassesValidationWithLocalValue()
    {
        var options = new SearchOptions { Environment = "LOCAL" };
        var results = ValidateOptions(options);
        Assert.Empty(results);
    }

    [Fact]
    public void Environment_PassesValidationWithAzureValue()
    {
        var options = new SearchOptions { Environment = "AZURE" };
        var results = ValidateOptions(options);
        Assert.Empty(results);
    }

    [Fact]
    public void Environment_PassesValidationWithLowercaseValue()
    {
        var options = new SearchOptions { Environment = "local" };
        var results = ValidateOptions(options);
        Assert.Empty(results);
    }

    [Fact]
    public void BindFromConfiguration_PopulatesEnvironment()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Search:Environment"] = "LOCAL"
            })
            .Build();

        var options = config.GetSection(SearchOptions.SectionName).Get<SearchOptions>();

        Assert.NotNull(options);
        Assert.Equal("LOCAL", options.Environment);
    }

    [Fact]
    public void BindFromConfiguration_PopulatesAzureEnvironment()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Search:Environment"] = "AZURE"
            })
            .Build();

        var options = config.GetSection(SearchOptions.SectionName).Get<SearchOptions>();

        Assert.NotNull(options);
        Assert.Equal("AZURE", options.Environment);
    }

    private static List<ValidationResult> ValidateOptions(SearchOptions options)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(options);
        Validator.TryValidateObject(options, ctx, results, validateAllProperties: true);
        return results;
    }
}
