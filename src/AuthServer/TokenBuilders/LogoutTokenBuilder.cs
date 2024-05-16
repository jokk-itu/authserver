using AuthServer.Cache;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Extensions;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class LogoutTokenBuilder : ITokenBuilder<LogoutTokenArguments>
{
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientJwkService _clientJwkService;

    public LogoutTokenBuilder(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        ICachedClientStore cachedClientStore,
        IClientJwkService clientJwkService)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _cachedClientStore = cachedClientStore;
        _clientJwkService = clientJwkService;
    }

    public async Task<string> BuildToken(LogoutTokenArguments arguments, CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedClientStore.Get(arguments.ClientId, cancellationToken);
        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Aud, arguments.ClientId },
            { ClaimNameConstants.Sid, arguments.SessionId },
            { ClaimNameConstants.Sub, arguments.UserId },
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.ClientId, arguments.ClientId },
            { ClaimNameConstants.Events, new Dictionary<string, object>
            {
                { "http://schemas.openid.net/event/backchannel-logout", new() }
            }}
        };

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(cachedClient.IdTokenSignedResponseAlg);
        var signingCredentials = new SigningCredentials(signingKey, cachedClient.IdTokenSignedResponseAlg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddSeconds(60),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.LogoutToken,
            Claims = claims
        };

        var encryptionKey = await _clientJwkService.GetEncryptionKey(arguments.ClientId, cancellationToken);
        var isEligibleForEncryption =
        encryptionKey is not null && cachedClient.IdTokenEncryptedResponseAlg is not null && cachedClient.IdTokenEncryptedResponseEnc is not null;

        if (isEligibleForEncryption)
        {
            var encryptingCredentials = new EncryptingCredentials(
                encryptionKey,
                cachedClient.IdTokenEncryptedResponseAlg!.GetDescription(),
                cachedClient.IdTokenEncryptedResponseEnc!.GetDescription());

            tokenDescriptor.EncryptingCredentials = encryptingCredentials;
        }

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}