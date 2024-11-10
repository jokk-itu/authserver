using System.Diagnostics;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class RefreshTokenBuilder : ITokenBuilder<RefreshTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly IMetricService _metricService;

    public RefreshTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _metricService = metricService;
    }

    public async Task<string> BuildToken(RefreshTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var grantQuery = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Select(x => new GrantQuery
            {
                AuthorizationGrant = x,
                Client = x.Client,
                SessionId = x.Session.Id,
                Subject = x.Subject
            })
            .SingleAsync(cancellationToken);

        if (grantQuery.Client.RequireReferenceToken)
        {
            var referenceToken = await BuildReferenceToken(arguments, grantQuery);
            stopWatch.Stop();
            _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.RefreshToken, TokenStructureTag.Reference);
            return referenceToken;
        }

        var jwt = await BuildStructuredToken(arguments, grantQuery);
        stopWatch.Stop();
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.RefreshToken, TokenStructureTag.Jwt);
        return jwt;
    }

    private async Task<string> BuildReferenceToken(RefreshTokenArguments arguments, GrantQuery grantQuery)
    {
        var refreshToken = new RefreshToken(
            grantQuery.AuthorizationGrant, grantQuery.Client.Id, _discoveryDocumentOptions.Value.Issuer,
            string.Join(' ', arguments.Scope), DateTime.UtcNow.AddSeconds(grantQuery.Client.RefreshTokenExpiration!.Value));

        await _identityContext
            .Set<RefreshToken>()
            .AddAsync(refreshToken);

        return refreshToken.Reference;
    }

    private async Task<string> BuildStructuredToken(RefreshTokenArguments arguments, GrantQuery grantQuery)
    {
        var now = DateTime.UtcNow;
        var refreshToken = new RefreshToken(
            grantQuery.AuthorizationGrant, grantQuery.Client.Id, _discoveryDocumentOptions.Value.Issuer,
            string.Join(' ', arguments.Scope), now.AddSeconds(grantQuery.Client.RefreshTokenExpiration!.Value));

        await _identityContext.Set<RefreshToken>().AddAsync(refreshToken);

        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Aud, grantQuery.Client.Id },
            { ClaimNameConstants.Sid, grantQuery.SessionId },
            { ClaimNameConstants.Sub, grantQuery.Subject },
            { ClaimNameConstants.Jti, refreshToken.Id },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.ClientId, grantQuery.Client.Id },
            { ClaimNameConstants.Scope, string.Join(' ', arguments.Scope) }
        };

        var signingKey = _jwksDocumentOptions.Value.GetTokenSigningKey();
        var signingCredentials = new SigningCredentials(signingKey.Key, signingKey.Alg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddSeconds(grantQuery.Client.RefreshTokenExpiration!.Value),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.RefreshToken,
            Claims = claims
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    private sealed class GrantQuery
    {
        public required AuthorizationGrant AuthorizationGrant { get; init; }
        public required Client Client { get; init; }
        public required string SessionId { get; init; }
        public required string Subject { get; init; }
    }
}