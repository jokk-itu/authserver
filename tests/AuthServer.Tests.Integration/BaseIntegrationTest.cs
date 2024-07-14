using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.Integration;
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        TestOutputHelper = testOutputHelper;
        ServiceProvider = _factory.Services.CreateScope().ServiceProvider;
    }

    protected HttpClient GetHttpClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });
}