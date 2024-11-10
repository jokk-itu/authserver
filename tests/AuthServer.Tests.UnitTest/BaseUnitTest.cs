using System.Security.Cryptography;
using AuthServer.Authentication.Abstractions;
using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Options;
using AuthServer.Tests.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest;

public abstract class BaseUnitTest
{
    private readonly SqliteConnection _connection;
    internal AuthorizationDbContext IdentityContext;

    protected ITestOutputHelper OutputHelper;
    protected JwtBuilder JwtBuilder;
    protected DiscoveryDocument DiscoveryDocument;
    protected JwksDocument JwksDocument;
    protected SigningAlg TokenSigningAlg = SigningAlg.RsaSha256;

    protected const string LevelOfAssuranceLow = AuthenticationContextReferenceConstants.LevelOfAssuranceLow;
    protected const string LevelOfAssuranceSubstantial = AuthenticationContextReferenceConstants.LevelOfAssuranceSubstantial;
    protected const string LevelOfAssuranceStrict = AuthenticationContextReferenceConstants.LevelOfAssuranceStrict;

    protected BaseUnitTest(ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        OutputHelper = outputHelper;
    }

    protected Task SaveChangesAsync() => IdentityContext.SaveChangesAsync();
    protected Task<Scope> GetScope(string name) => IdentityContext.Set<Scope>().SingleAsync(x => x.Name == name);
    protected Task<GrantType> GetGrantType(string name) => IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == name);
    protected Task<ResponseType> GetResponseType(string name) => IdentityContext.Set<ResponseType>().SingleAsync(x => x.Name == name);
    protected Task<AuthenticationMethodReference> GetAuthenticationMethodReference(string name) => IdentityContext.Set<AuthenticationMethodReference>().SingleAsync(x => x.Name == name);
    protected Task<AuthenticationContextReference> GetAuthenticationContextReference(string name) => IdentityContext.Set<AuthenticationContextReference>().SingleAsync(x => x.Name == name);

    protected async Task AddEntity<T>(T entity) where T : class
    {
        await IdentityContext.Set<T>().AddAsync(entity);
        await IdentityContext.SaveChangesAsync();
    }

    protected IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<DiscoveryDocument>().Configure(discoveryDocument =>
        {
            discoveryDocument.Issuer = "https://localhost:5000";
            discoveryDocument.ClaimsSupported = ClaimNameConstants.SupportedEndUserClaims;
            discoveryDocument.AcrValuesSupported = [LevelOfAssuranceLow, LevelOfAssuranceSubstantial, LevelOfAssuranceStrict];
        });
        services.AddOptions<JwksDocument>().Configure(jwksDocument =>
        {
            var rsa = RSA.Create(3072);
            var rsaSecurityKey = new RsaSecurityKey(rsa)
            {
                KeyId = Guid.NewGuid().ToString()
            };

            var ecdsa = ECDsa.Create();
            var ecdsaSecurityKey = new ECDsaSecurityKey(ecdsa)
            {
                KeyId = Guid.NewGuid().ToString()
            };

            jwksDocument.SigningKeys =
            [
                new(rsaSecurityKey, SigningAlg.RsaSha256),
                new(rsaSecurityKey, SigningAlg.RsaSha384),
                new(rsaSecurityKey, SigningAlg.RsaSha512),
                new(rsaSecurityKey, SigningAlg.RsaSsaPssSha256),
                new(rsaSecurityKey, SigningAlg.RsaSsaPssSha384),
                new(rsaSecurityKey, SigningAlg.RsaSsaPssSha512),
                new(ecdsaSecurityKey, SigningAlg.EcdsaSha256),
                new(ecdsaSecurityKey, SigningAlg.EcdsaSha384),
                new(ecdsaSecurityKey, SigningAlg.EcdsaSha512),
            ];

            jwksDocument.EncryptionKeys =
            [
                new(rsaSecurityKey, EncryptionAlg.RsaOAEP),
                new(rsaSecurityKey, EncryptionAlg.RsaPKCS1),
                new(ecdsaSecurityKey, EncryptionAlg.EcdhEsA128KW),
                new(ecdsaSecurityKey, EncryptionAlg.EcdhEsA192KW),
                new(ecdsaSecurityKey, EncryptionAlg.EcdhEsA256KW)
            ];

            jwksDocument.GetTokenSigningKey =
                () => jwksDocument.SigningKeys.Single(x => x.Alg == TokenSigningAlg);
        });
        services.AddOptions<UserInteraction>().Configure(userInteraction =>
        {
            userInteraction.LoginUri = "https://localhost:5000/login";
            userInteraction.ConsentUri = "https://localhost:5000/consent";
            userInteraction.AccountSelectionUri = "https://localhost:5000/select-account";
            userInteraction.EndSessionUri = "https://localhost:5000/logout";
        });
        services.AddAuthServer((_, contextOptions) =>
        {
            contextOptions.UseSqlite(_connection,
                optionsBuilder =>
                {
                    optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
        });
        services.AddScoped<IDistributedCache, InMemoryCache>();
        services.AddScoped<IUserClaimService, UserClaimService>();
        services.AddScoped<IAuthenticatedUserAccessor, AuthenticatedUserAccessor>();
        services.AddScoped<IAuthenticationContextReferenceResolver, AuthenticationContextReferenceResolver>();

        return services;
    }

    protected IServiceProvider BuildServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        configure?.Invoke(services);
        var serviceProvider = services.BuildServiceProvider();
        var identityContext = serviceProvider.GetRequiredService<AuthorizationDbContext>();
        IdentityContext = identityContext;
        IdentityContext.Database.EnsureCreated();

        CreateAuthenticationContextReferences().GetAwaiter().GetResult();

        var discoveryDocument = serviceProvider.GetRequiredService<IOptionsSnapshot<DiscoveryDocument>>();
        DiscoveryDocument = discoveryDocument.Value;

        var jwksDocument = serviceProvider.GetRequiredService<IOptionsSnapshot<JwksDocument>>();
        JwksDocument = jwksDocument.Value;

        JwtBuilder = new JwtBuilder(DiscoveryDocument, JwksDocument);

        return serviceProvider;
    }

    private async Task CreateAuthenticationContextReferences()
    {
        var authenticationContextReferenceLow = new AuthenticationContextReference(LevelOfAssuranceLow);
        var authenticationContextReferenceSubstantial = new AuthenticationContextReference(LevelOfAssuranceSubstantial);
        var authenticationContextReferenceStrict = new AuthenticationContextReference(LevelOfAssuranceStrict);
        await AddEntity(authenticationContextReferenceLow);
        await AddEntity(authenticationContextReferenceSubstantial);
        await AddEntity(authenticationContextReferenceStrict);
    }
}