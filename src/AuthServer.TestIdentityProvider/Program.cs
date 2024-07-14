using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Enums;
using AuthServer.Extensions;
using AuthServer.Introspection.Abstractions;
using AuthServer.TestIdentityProvider.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AuthServer.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthentication();

builder.Services
    .AddOptions<DiscoveryDocument>()
    .Configure(options =>
    {
        var identitySection = builder.Configuration.GetSection("Identity");
        options.Issuer = identitySection.GetValue<string>("Issuer")!;
        options.ClaimsSupported = ClaimNameConstants.SupportedEndUserClaims;
        options.RequestParameterSupported = true;
        options.RequestUriParameterSupported = true;

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

builder.Services
    .AddOptions<JwksDocument>()
    .Configure(options =>
    {
        var ecdsa = ECDsa.Create();
        var rsa = RSA.Create(3072);

        var ecdsaSecurityKey = new ECDsaSecurityKey(ecdsa);
        var rsaSecurityKey = new RsaSecurityKey(rsa);

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

builder.Services.AddSingleton<IDistributedCache, MockDistributedCache>();
builder.Services.AddScoped<IUserClaimService, MockUserClaimService>();
builder.Services.AddScoped<IUsernameResolver, MockUsernameResolver>();

builder.Services.AddAuthServer(dbContextConfigurator =>
{
    dbContextConfigurator.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        optionsBuilder => optionsBuilder.MigrationsAssembly("AuthServer.TestIdentityProvider"));
});

builder.Services.AddHttpContextAccessor();


var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapAuthorizeEndpoint();
app.MapPostRegisterEndpoint();
app.MapPutRegisterEndpoint();
app.MapDeleteRegisterEndpoint();
app.MapGetRegisterEndpoint();
app.MapUserinfoEndpoint();
app.MapTokenEndpoint();
app.MapRevocationEndpoint();
app.MapIntrospectionEndpoint();
app.MapDiscoveryDocumentEndpoint();
app.MapJwksDocumentEndpoint();

app.Run();

// Used ONLY for Tests.Integration
public partial class Program
{
}