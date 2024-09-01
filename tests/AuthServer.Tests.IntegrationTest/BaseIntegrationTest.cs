using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Options;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;

[Collection("IntegrationTest")]
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly IServiceProvider ServiceProvider;

    private readonly IOptionsMonitor<DiscoveryDocument> _discoveryDocumentOptions;
    protected DiscoveryDocument DiscoveryDocument => _discoveryDocumentOptions.CurrentValue;

    private readonly IOptionsMonitor<UserInteraction> _userInteractionOptions;
    protected UserInteraction UserInteraction => _userInteractionOptions.CurrentValue;

    private readonly IOptionsMonitor<JwksDocument> _jwksDocumentOptions;
    protected JwksDocument JwksDocument => _jwksDocumentOptions.CurrentValue;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        TestOutputHelper = testOutputHelper;
        ServiceProvider = _factory.Services.CreateScope().ServiceProvider;

        _discoveryDocumentOptions = _factory.Services.GetRequiredService<IOptionsMonitor<DiscoveryDocument>>();
        _userInteractionOptions = _factory.Services.GetRequiredService<IOptionsMonitor<UserInteraction>>();
        _jwksDocumentOptions = _factory.Services.GetRequiredService<IOptionsMonitor<JwksDocument>>();

        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.EnsureDeleted();
        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.Migrate();
    }

    protected HttpClient GetHttpClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    protected async Task<string> AddWeatherReadScope()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        const string scopeName = "weather:read";
        var scope = new Scope(scopeName);

        dbContext.Add(scope);
        await dbContext.SaveChangesAsync();

        return scopeName;
    }

    protected async Task<Client> AddWeatherClient()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        var weatherScope = await dbContext.Set<Scope>().SingleAsync(x => x.Name == "weather:read");
        var client = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            Scopes = [ weatherScope ],
            ClientUri = "https://weather.authserver.dk"
        };
        dbContext.Add(client);
        await dbContext.SaveChangesAsync();

        return client;
    }
}