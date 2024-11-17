using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Enums;
using AuthServer.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AuthServer.Tests.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Logging;
using AuthServer.Options;
using AuthServer.Authorize.Abstractions;
using AuthServer.Authentication.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
        options => { options.Cookie.Name = "AuthServer.Identity"; });

builder.Services
    .AddOptions<DiscoveryDocument>()
    .Configure(options =>
    {
        var identitySection = builder.Configuration.GetSection("Identity");
        options.Issuer = identitySection.GetValue<string>("Issuer")!;
        options.ClaimsSupported = ClaimNameConstants.SupportedEndUserClaims;
        options.AcrValuesSupported =
        [
            AuthenticationContextReferenceConstants.LevelOfAssuranceLow,
            AuthenticationContextReferenceConstants.LevelOfAssuranceSubstantial,
            AuthenticationContextReferenceConstants.LevelOfAssuranceStrict
        ];
        options.ScopesSupported = identitySection.GetSection("ScopesSupported").Get<ICollection<string>>() ?? [];

        ICollection<string> signingAlgorithms =
            [JwsAlgConstants.RsaSha256, JwsAlgConstants.EcdsaSha256, JwsAlgConstants.RsaSsaPssSha256];
        ICollection<string> encryptionAlgorithms =
            [JweAlgConstants.EcdhEsA128KW, JweAlgConstants.RsaOAEP, JweAlgConstants.RsaPKCS1];
        ICollection<string> encoderAlgorithms = [JweEncConstants.Aes128CbcHmacSha256];

        options.TokenEndpointAuthSigningAlgValuesSupported = signingAlgorithms;
        options.IdTokenSigningAlgValuesSupported = signingAlgorithms;
        options.IntrospectionEndpointAuthSigningAlgValuesSupported = signingAlgorithms;
        options.RequestObjectSigningAlgValuesSupported = signingAlgorithms;
        options.RevocationEndpointAuthSigningAlgValuesSupported = signingAlgorithms;
        options.UserinfoSigningAlgValuesSupported = signingAlgorithms;

        options.IdTokenEncryptionAlgValuesSupported = encryptionAlgorithms;
        options.RequestObjectEncryptionAlgValuesSupported = encryptionAlgorithms;
        options.UserinfoEncryptionAlgValuesSupported = encryptionAlgorithms;

        options.IdTokenEncryptionEncValuesSupported = encoderAlgorithms;
        options.RequestObjectEncryptionEncValuesSupported = encoderAlgorithms;
        options.UserinfoEncryptionEncValuesSupported = encoderAlgorithms;
    });

var ecdsa = ECDsa.Create();
var rsa = RSA.Create(3072);

var ecdsaSecurityKey = new ECDsaSecurityKey(ecdsa)
{
    KeyId = Guid.NewGuid().ToString()
};
var rsaSecurityKey = new RsaSecurityKey(rsa)
{
    KeyId = Guid.NewGuid().ToString()
};
builder.Services
    .AddOptions<JwksDocument>()
    .Configure(options =>
    {
        options.EncryptionKeys =
        [
            new JwksDocument.EncryptionKey(ecdsaSecurityKey, EncryptionAlg.EcdhEsA128KW),
            new JwksDocument.EncryptionKey(ecdsaSecurityKey, EncryptionAlg.EcdhEsA192KW),
            new JwksDocument.EncryptionKey(ecdsaSecurityKey, EncryptionAlg.EcdhEsA256KW),
            new JwksDocument.EncryptionKey(rsaSecurityKey, EncryptionAlg.RsaOAEP),
            new JwksDocument.EncryptionKey(rsaSecurityKey, EncryptionAlg.RsaPKCS1),
        ];
        options.SigningKeys =
        [
            new JwksDocument.SigningKey(ecdsaSecurityKey, SigningAlg.EcdsaSha256),
            new JwksDocument.SigningKey(ecdsaSecurityKey, SigningAlg.EcdsaSha384),
            new JwksDocument.SigningKey(ecdsaSecurityKey, SigningAlg.EcdsaSha512),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSha256),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSha384),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSha512),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSsaPssSha256),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSsaPssSha384),
            new JwksDocument.SigningKey(rsaSecurityKey, SigningAlg.RsaSsaPssSha512),
        ];

        options.GetTokenSigningKey =
            () => options.SigningKeys.Single(x => x.Alg == SigningAlg.RsaSha256);
    });

builder.Services
    .AddOptions<UserInteraction>()
    .Configure(options =>
    {
        var identity = builder.Configuration.GetSection("Identity");
        options.AccountSelectionUri = identity.GetValue<string>("AccountSelectionUri")!;
        options.ConsentUri = identity.GetValue<string>("ConsentUri")!;
        options.LoginUri = identity.GetValue<string>("LoginUri")!;
        options.EndSessionUri = identity.GetValue<string>("EndSessionUri")!;
    });

builder.Services.AddSingleton<IDistributedCache, InMemoryCache>();
builder.Services.AddScoped<IUserClaimService, UserClaimService>();
builder.Services.AddScoped<IAuthenticatedUserAccessor, AuthenticatedUserAccessor>();
builder.Services.AddScoped<IAuthenticationContextReferenceResolver, AuthenticationContextReferenceResolver>();

builder.Services.AddAuthServer(
    (_, dbContextConfigurator) =>
    {
        dbContextConfigurator.UseSqlServer(
            builder.Configuration.GetConnectionString("Default"),
            optionsBuilder =>
            {
                optionsBuilder.MigrationsAssembly("AuthServer.TestIdentityProvider");
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();


var app = builder.Build();

IdentityModelEventSource.ShowPII = true;

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthServer();
app.MapRazorPages();

app.Run();

// Used ONLY for Tests.Integration
public partial class Program
{
}