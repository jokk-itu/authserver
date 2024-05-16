using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Extensions;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class UserinfoTokenBuilder : ITokenBuilder<UserinfoTokenArguments>
{
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly ITokenSecurityService _tokenSecurityService;

    public UserinfoTokenBuilder(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        ITokenSecurityService tokenSecurityService)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _tokenSecurityService = tokenSecurityService;
    }

    public async Task<string> BuildToken(UserinfoTokenArguments arguments, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(arguments.SigningAlg);
        var signingCredentials = new SigningCredentials(signingKey, arguments.SigningAlg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddHours(1),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            Audience = arguments.ClientId,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.UserinfoToken,
            Claims = arguments.EndUserClaims.ToDictionary()
        };

        if (arguments.EncryptionAlg is not null && arguments.EncryptionEnc is not null)
        {
            tokenDescriptor.EncryptingCredentials = await _tokenSecurityService.GetEncryptingCredentials(
                arguments.ClientId,
                arguments.EncryptionAlg.Value,
                arguments.EncryptionEnc.Value,
                cancellationToken);
        }

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}