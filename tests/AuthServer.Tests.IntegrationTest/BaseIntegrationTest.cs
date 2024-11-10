using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Options;
using AuthServer.Tests.Core;
using AuthServer.Tests.IntegrationTest.EndpointBuilders;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;

[Collection("IntegrationTest")]
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly AuthorizeEndpointBuilder AuthorizeEndpointBuilder;
    protected readonly RegisterEndpointBuilder RegisterEndpointBuilder;
    protected readonly IntrospectionEndpointBuilder IntrospectionEndpointBuilder;
    protected readonly RevocationEndpointBuilder RevocationEndpointBuilder;
    protected readonly UserinfoEndpointBuilder UserinfoEndpointBuilder;

    protected TokenEndpointBuilder TokenEndpointBuilder => new(GetHttpClient(), DiscoveryDocument, JwksDocument, TestOutputHelper);

    private readonly IOptionsMonitor<DiscoveryDocument> _discoveryDocumentOptions;
    protected DiscoveryDocument DiscoveryDocument => _discoveryDocumentOptions.CurrentValue;

    private readonly IOptionsMonitor<UserInteraction> _userInteractionOptions;
    protected UserInteraction UserInteraction => _userInteractionOptions.CurrentValue;

    private readonly IOptionsMonitor<JwksDocument> _jwksDocumentOptions;
    protected JwksDocument JwksDocument => _jwksDocumentOptions.CurrentValue;

    protected JwtBuilder JwtBuilder => new (DiscoveryDocument, JwksDocument);

    protected const string LevelOfAssuranceLow = AuthenticationContextReferenceConstants.LevelOfAssuranceLow;
    protected const string LevelOfAssuranceSubstantial = AuthenticationContextReferenceConstants.LevelOfAssuranceSubstantial;
    protected const string LevelOfAssuranceStrict = AuthenticationContextReferenceConstants.LevelOfAssuranceStrict;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Integration");
            builder.ConfigureServices(services =>
            {
                var authenticatedUserAccessor = new Mock<IAuthenticatedUserAccessor>();
                authenticatedUserAccessor
                    .Setup(x => x.CountAuthenticatedUsers())
                    .ReturnsAsync(2);

                services.AddScopedMock(authenticatedUserAccessor);
            });
        });

        TestOutputHelper = testOutputHelper;
        ServiceProvider = _factory.Services.CreateScope().ServiceProvider;

        _discoveryDocumentOptions = _factory.Services.GetRequiredService<IOptionsMonitor<DiscoveryDocument>>();
        _userInteractionOptions = _factory.Services.GetRequiredService<IOptionsMonitor<UserInteraction>>();
        _jwksDocumentOptions = _factory.Services.GetRequiredService<IOptionsMonitor<JwksDocument>>();

        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.EnsureDeleted();
        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Database.Migrate();

        var dataProtectionProvider = ServiceProvider.GetRequiredService<IDataProtectionProvider>();
        AuthorizeEndpointBuilder = new AuthorizeEndpointBuilder(
            GetHttpClient(),
            dataProtectionProvider,
            DiscoveryDocument,
            JwksDocument,
            TestOutputHelper);

        RegisterEndpointBuilder = new RegisterEndpointBuilder(
            GetHttpClient(),
            DiscoveryDocument,
            JwksDocument,
            TestOutputHelper);

        IntrospectionEndpointBuilder = new IntrospectionEndpointBuilder(
            GetHttpClient(),
            DiscoveryDocument,
            JwksDocument,
            TestOutputHelper);

        RevocationEndpointBuilder = new RevocationEndpointBuilder(
            GetHttpClient(),
            DiscoveryDocument,
            JwksDocument,
            TestOutputHelper);

        UserinfoEndpointBuilder = new UserinfoEndpointBuilder(
            GetHttpClient(),
            DiscoveryDocument,
            JwksDocument,
            TestOutputHelper);
    }

    protected HttpClient GetHttpClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    protected string GetUserinfoScope() =>
        ServiceProvider.GetRequiredService<AuthorizationDbContext>().Set<Scope>().Single(x => x.Name == ScopeConstants.UserInfo).Name;

    protected async Task<string> AddWeatherReadScope()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        const string scopeName = "weather:read";
        var scope = new Scope(scopeName);

        await dbContext.AddAsync(scope);
        await dbContext.SaveChangesAsync();

        return scopeName;
    }

    protected async Task<Client> AddWeatherClient()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        var weatherScope = await dbContext.Set<Scope>().SingleAsync(x => x.Name == "weather:read");
        var client = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            Scopes = [weatherScope],
            ClientUri = "https://weather.authserver.dk"
        };

        await dbContext.AddAsync(client);
        await dbContext.SaveChangesAsync();

        return client;
    }

    protected async Task<Client> AddIdentityProviderClient()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        var userinfoScope = await dbContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.UserInfo);
        var client = new Client("identity-provider", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            Scopes = [userinfoScope],
            ClientUri = "https://localhost:7254"
        };

        await dbContext.AddAsync(client);
        await dbContext.SaveChangesAsync();

        return client;
    }

    protected async Task AddUser()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        var subjectIdentifier = new SubjectIdentifier();
        typeof(SubjectIdentifier)
            .GetProperty(nameof(SubjectIdentifier.Id))!
            .SetValue(subjectIdentifier, UserConstants.SubjectIdentifier);

        await dbContext.AddAsync(subjectIdentifier);
        await dbContext.SaveChangesAsync();
    }

    protected async Task AddAuthenticationContextReferences()
    {
        var dbContext = ServiceProvider.GetRequiredService<AuthorizationDbContext>();

        var authenticationContextReferenceLow = new AuthenticationContextReference(LevelOfAssuranceLow);
        var authenticationContextReferenceSubstantial = new AuthenticationContextReference(LevelOfAssuranceSubstantial);
        var authenticationContextReferenceStrict = new AuthenticationContextReference(LevelOfAssuranceStrict);

        dbContext.AddRange(
            authenticationContextReferenceLow,
            authenticationContextReferenceSubstantial,
            authenticationContextReferenceStrict);

        await dbContext.SaveChangesAsync();
    }
}