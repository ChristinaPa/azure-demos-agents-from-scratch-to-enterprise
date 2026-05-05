using System.Net;
using ASE.EnterpriseApi.Options;
using ASE.EnterpriseApi.Routes;
using ASE.Libraries.General;
using ASE.Libraries.Search;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;

#region Server Configuration

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(settings => settings.AddServerHeader = false);
builder.Services.AddTransient<ILogger>(p =>
{
    var loggerFactory = p.GetRequiredService<ILoggerFactory>();
    return loggerFactory.CreateLogger("AgentEnterpriseApi");
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.AddOptions<CorsOptions>()
    .BindConfiguration(CorsOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<SearchOptions>()
    .BindConfiguration(SearchOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var corsConfig = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
                 ?? throw new InvalidOperationException("Cors configuration section is missing.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.WithOrigins(corsConfig.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddHealthChecks();

#endregion

#region MCP configuration

builder.Services
    .AddMcpServer()
    .WithHttpTransport(o => o.Stateless = true)
    .WithToolsFromAssembly();

#endregion

var searchConfig = builder.Configuration.GetSection(SearchOptions.SectionName).Get<SearchOptions>()
    ?? throw new InvalidOperationException("Search configuration section is missing.");

if (searchConfig.Environment.Equals("LOCAL", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<ISearchService, DocumentSearchAdapter>();
}
else
{
    builder.Services.AddScoped<ISearchService, AzureSearchDocumentSearchAdapter>();
}

#region Run Configuration

var app = builder.Build();
app.UseForwardedHeaders();
app.UseCors("AllowVueApp");
//api routes and implementation
app.MapGroup(RouteNames.BasicRoute)
    .MapBasicEnterpriseApi();
app.MapGroup(RouteNames.AdvancedRoute)
    .MapAdvancedEnterpriseApi();
    
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        if (exception != null)
        {
            var message = $"{exception.Error.Message}";
            await context.Response.WriteAsync(message).ConfigureAwait(false);
        }
    });
});
app.MapHealthChecks("/health");
app.MapMcp("/mcp");
app.Run();

#endregion

namespace ASE.EnterpriseApi
{
    public partial class Program { }
}
