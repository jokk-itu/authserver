using System.Diagnostics;
using System.Text.Json;
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

internal class GrantAccessTokenBuilder : ITokenBuilder<GrantAccessTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocument;
    private readonly IMetricService _metricService;

    public GrantAccessTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocument,
        IMetricService metricService)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocument = jwksDocument;
        _metricService = metricService;
    }

    public async Task<string> BuildToken(GrantAccessTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var grantQuery = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Select(x => new GrantQuery
            {
                AuthorizationGrant = x,
                Client = x.Client,
                Subject = x.Subject,
                SessionId = x.Session.Id
            })
            .SingleAsync(cancellationToken);

        if (grantQuery.Client.RequireReferenceToken)
        {
            var referenceToken = await BuildReferenceToken(arguments, grantQuery);
            stopWatch.Stop();
            _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.AccessToken, TokenStructureTag.Reference);
            return referenceToken;
        }

        var jwt = BuildStructuredToken(arguments, grantQuery);
        stopWatch.Stop();
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.AccessToken, TokenStructureTag.Jwt);
        return jwt;
    }

    private string BuildStructuredToken(GrantAccessTokenArguments arguments, GrantQuery grantQuery)
    {
        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.Scope, string.Join(' ', arguments.Scope) },
            { ClaimNameConstants.Aud, JsonSerializer.SerializeToElement(arguments.Resource) },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.Sub, grantQuery.Subject },
            { ClaimNameConstants.Sid, grantQuery.SessionId },
            { ClaimNameConstants.ClientId, grantQuery.Client.Id }
        };

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocument.Value.GetTokenSigningKey();
        var signingCredentials = new SigningCredentials(signingKey.Key, signingKey.Alg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddSeconds(grantQuery.Client.AccessTokenExpiration),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.AccessToken,
            Claims = claims
        };
        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }

    private async Task<string> BuildReferenceToken(GrantAccessTokenArguments arguments, GrantQuery grantQuery)
    {
        var accessToken = new GrantAccessToken(grantQuery.AuthorizationGrant,
            string.Join(' ', arguments.Resource), _discoveryDocumentOptions.Value.Issuer,
            string.Join(' ', arguments.Scope), DateTime.UtcNow.AddSeconds(grantQuery.Client.AccessTokenExpiration));

        await _identityContext.Set<GrantAccessToken>().AddAsync(accessToken);
        return accessToken.Reference;
    }

    private sealed class GrantQuery
    {
        public required AuthorizationGrant AuthorizationGrant { get; init; }
        public required Client Client { get; init; }
        public required string SessionId { get; init; }
        public required string Subject { get; init; }
    }
}