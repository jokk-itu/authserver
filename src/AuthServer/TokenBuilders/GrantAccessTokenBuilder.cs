using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class GrantAccessTokenBuilder : ITokenBuilder<GrantAccessTokenArguments>
{
    private readonly IdentityContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocument;

    public GrantAccessTokenBuilder(
        IdentityContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocument)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocument = jwksDocument;
    }

    public async Task<string> BuildToken(GrantAccessTokenArguments arguments, CancellationToken cancellationToken)
    {
        var grantQuery = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Select(x => new GrantQuery
            {
                AuthorizationGrant = x,
                Client = x.Client,
                SubjectId = x.SubjectIdentifier.Id,
                SessionId = x.Session.Id
            })
            .SingleAsync(cancellationToken);

        if (grantQuery.Client.RequireReferenceToken)
        {
            return await BuildReferenceToken(arguments, grantQuery);
        }

        return await BuildStructuredToken(arguments, grantQuery);
    }

    private async Task<string> BuildStructuredToken(GrantAccessTokenArguments arguments, GrantQuery grantQuery)
    {
        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.Scope, arguments.Scope },
            { ClaimNameConstants.Aud, arguments.Resource },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.Sub, grantQuery.SubjectId },
            { ClaimNameConstants.Sid, grantQuery.SessionId },
            { ClaimNameConstants.ClientId, grantQuery.Client.Id }
        };

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocument.Value.GetSigningKey(grantQuery.Client.TokenEndpointAuthSigningAlg);
        var signingCredentials = new SigningCredentials(signingKey, grantQuery.Client.TokenEndpointAuthSigningAlg.GetDescription());

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
        public required string SubjectId { get; init; }
    }
}