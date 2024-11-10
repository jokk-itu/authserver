using System.Diagnostics;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Extensions;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
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
    private readonly ITokenSecurityService _tokenSecurityService;
    private readonly IMetricService _metricService;

    public LogoutTokenBuilder(
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        ICachedClientStore cachedClientStore,
        ITokenSecurityService tokenSecurityService,
        IMetricService metricService)
    {
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _cachedClientStore = cachedClientStore;
        _tokenSecurityService = tokenSecurityService;
        _metricService = metricService;
    }

    public async Task<string> BuildToken(LogoutTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var cachedClient = await _cachedClientStore.Get(arguments.ClientId, cancellationToken);
        var claims = new Dictionary<string, object?>
        {
            { ClaimNameConstants.Aud, arguments.ClientId },
            { ClaimNameConstants.Sid, arguments.SessionId },
            { ClaimNameConstants.Sub, arguments.SubjectIdentifier },
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.ClientId, arguments.ClientId },
            {
                ClaimNameConstants.Events, new Dictionary<string, object>
                {
                    { "http://schemas.openid.net/event/backchannel-logout", new Dictionary<string, object>() }
                }
            }
        };

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(cachedClient.IdTokenSignedResponseAlg!.Value);
        var signingCredentials =
            new SigningCredentials(signingKey, cachedClient.IdTokenSignedResponseAlg.GetDescription());

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

        if (cachedClient.IdTokenEncryptedResponseAlg is not null &&
            cachedClient.IdTokenEncryptedResponseEnc is not null)
        {
            tokenDescriptor.EncryptingCredentials = await _tokenSecurityService.GetEncryptingCredentials(
                arguments.ClientId,
                cachedClient.IdTokenEncryptedResponseAlg.Value,
                cachedClient.IdTokenEncryptedResponseEnc.Value,
                cancellationToken);
        }

        var tokenHandler = new JsonWebTokenHandler();
        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.LogoutToken, TokenStructureTag.Jwt);
        return jwt;
    }
}