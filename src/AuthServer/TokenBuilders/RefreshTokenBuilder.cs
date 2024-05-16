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

internal class RefreshTokenBuilder : ITokenBuilder<RefreshTokenArguments>
{
    private readonly IdentityContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;

    public RefreshTokenBuilder(
        IdentityContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
    }

    public async Task<string> BuildToken(RefreshTokenArguments arguments, CancellationToken cancellationToken)
    {
        var grantQuery = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Select(x => new GrantQuery
            {
                AuthorizationGrant = x,
                Client = x.Client,
                SessionId = x.Session.Id,
                SubjectId = x.SubjectIdentifier.Id
            })
            .SingleAsync(cancellationToken);

        if (grantQuery.Client.RequireReferenceToken)
        {
            return await BuildReferenceToken(arguments, grantQuery);
        }

        return await BuildStructuredToken(arguments, grantQuery);
    }

    private async Task<string> BuildReferenceToken(RefreshTokenArguments arguments, GrantQuery grantQuery)
    {
        var refreshToken = new RefreshToken(
            grantQuery.AuthorizationGrant, grantQuery.Client.Id, _discoveryDocumentOptions.Value.Issuer,
            string.Join(' ', arguments.Scope), DateTime.UtcNow.AddSeconds(grantQuery.Client.RefreshTokenExpiration));

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
            string.Join(' ', arguments.Scope), now.AddSeconds(grantQuery.Client.RefreshTokenExpiration));

        await _identityContext.Set<RefreshToken>().AddAsync(refreshToken);

        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Aud, grantQuery.Client.Id },
            { ClaimNameConstants.Sid, grantQuery.SessionId },
            { ClaimNameConstants.Sub, grantQuery.SubjectId },
            { ClaimNameConstants.Jti, refreshToken.Id },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.ClientId, grantQuery.Client.Id }
        };

        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(grantQuery.Client.TokenEndpointAuthSigningAlg);
        var signingCredentials = new SigningCredentials(signingKey, grantQuery.Client.TokenEndpointAuthSigningAlg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddSeconds(grantQuery.Client.RefreshTokenExpiration),
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
        public required string SubjectId { get; init; }
    }
}