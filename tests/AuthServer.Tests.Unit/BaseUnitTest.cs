using System.Security.Cryptography;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Introspection.Abstractions;
using AuthServer.Tests.Core;
using AuthServer.Tests.Core.EntityBuilders;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit;

public abstract class BaseUnitTest
{
    private readonly SqliteConnection _connection;
    internal AuthorizationDbContext IdentityContext;

    protected ITestOutputHelper OutputHelper;
    protected ClientJwtBuilder ClientJwtBuilder;
    protected DiscoveryDocument DiscoveryDocument;
    protected JwksDocument JwksDocument;
    protected ClientBuilder ClientBuilder;
    protected SigningAlg TokenSigningAlg = SigningAlg.RsaSha256;

    protected BaseUnitTest(ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        OutputHelper = outputHelper;
    }

    protected Task SaveChangesAsync() => IdentityContext.SaveChangesAsync();
    protected DbSet<T> DbSet<T>() where T : class => IdentityContext.Set<T>();

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
        });
        services.AddOptions<JwksDocument>().Configure(jwksDocument =>
        {
            var rsa = RSA.Create(3072);
            var rsaSecurityKey = new RsaSecurityKey(rsa);

            var ecdsa = ECDsa.Create();
            var ecdsaSecurityKey = new ECDsaSecurityKey(ecdsa);

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
        services.AddAuthServer(contextOptions =>
        {
            contextOptions.UseSqlite(_connection,
                optionsBuilder => { optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); });
        });
        services.AddScoped<IUsernameResolver, UsernameResolver>();
        services.AddScoped<IDistributedCache, InMemoryCache>();
        services.AddScoped<IUserClaimService, UserClaimService>();

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

        var discoveryDocument = serviceProvider.GetRequiredService<IOptionsSnapshot<DiscoveryDocument>>();
        DiscoveryDocument = discoveryDocument.Value;

        var jwksDocument = serviceProvider.GetRequiredService<IOptionsSnapshot<JwksDocument>>();
        JwksDocument = jwksDocument.Value;

        ClientJwtBuilder = new ClientJwtBuilder(DiscoveryDocument);
        ClientBuilder = new ClientBuilder(identityContext);

        return serviceProvider;
    }
}