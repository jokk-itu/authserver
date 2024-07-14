using AuthServer.Constants;
using AuthServer.Core.Discovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Options;
internal class ConfigureJwtBearerOptions : IConfigureOptions<JwtBearerOptions>
{
    private readonly IOptionsMonitor<JwksDocument> _jwkDocumentOptions;
    private readonly IOptionsMonitor<DiscoveryDocument> _discoveryDocumentOptions;

    public ConfigureJwtBearerOptions(
        IOptionsMonitor<JwksDocument> jwkDocumentOptions,
        IOptionsMonitor<DiscoveryDocument> discoveryDocumentOptions)
    {
        _jwkDocumentOptions = jwkDocumentOptions;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    private JwksDocument JwkDocument => _jwkDocumentOptions.CurrentValue;
    private DiscoveryDocument DiscoveryDocument => _discoveryDocumentOptions.CurrentValue;

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = DiscoveryDocument.Issuer;
        options.Authority = DiscoveryDocument.Issuer;
        options.Challenge = JwtBearerDefaults.AuthenticationScheme;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.FromSeconds(0),
            IssuerSigningKeys = JwkDocument.SigningKeys.Select(x => x.Key),
            TokenDecryptionKeys = JwkDocument.EncryptionKeys.Select(x => x.Key),
            NameClaimType = ClaimNameConstants.Name,
            RoleClaimType = ClaimNameConstants.Roles,
        };
    }
}