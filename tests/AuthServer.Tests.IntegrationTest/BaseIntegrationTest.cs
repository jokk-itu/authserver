using AuthServer.Core;
using AuthServer.Core.Discovery;
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

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        TestOutputHelper = testOutputHelper;
        ServiceProvider = _factory.Services.CreateScope().ServiceProvider;

        _discoveryDocumentOptions = _factory.Services.GetRequiredService<IOptionsMonitor<DiscoveryDocument>>();
        _userInteractionOptions = _factory.Services.GetRequiredService<IOptionsMonitor<UserInteraction>>();

        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.EnsureDeleted();
        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.Migrate();
    }

    protected HttpClient GetHttpClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
}