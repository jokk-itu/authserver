using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;
internal class IdTokenBuilder : ITokenBuilder<IdTokenArguments>
{
    private readonly IdentityContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly ITokenSecurityService _tokenSecurityService;
    private readonly IUserClaimService _userClaimService;

    public IdTokenBuilder(
        IdentityContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        ITokenSecurityService tokenSecurityService,
        IUserClaimService userClaimService)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _tokenSecurityService = tokenSecurityService;
        _userClaimService = userClaimService;
    }

    public async Task<string> BuildToken(IdTokenArguments arguments, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Include(x => x.Nonces)
            .Select(x => new
            {
                ClientId = x.Client.Id,
                SessionId = x.Session.Id,
                PublicSubjectId = x.Session.PublicSubjectIdentifier.Id,
                GrantSubjectId = x.SubjectIdentifier.Id,
                SigningAlg = x.Client.IdTokenSignedResponseAlg,
                EncryptionAlg = x.Client.IdTokenEncryptedResponseAlg,
                EncryptionEnc = x.Client.IdTokenEncryptedResponseEnc,
                Nonce = x.Nonces.OrderByDescending(y => y.IssuedAt).First()
            })
            .SingleAsync(cancellationToken);

        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Sub, query.GrantSubjectId },
            { ClaimNameConstants.Aud, query.ClientId },
            { ClaimNameConstants.Sid, query.SessionId },
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.Nonce, query.Nonce.Value },
            { ClaimNameConstants.ClientId, query.ClientId },
            { ClaimNameConstants.Azp, query.ClientId }
            // TODO acr from claims in arguments, which must be extended
            // TODO amr from arguments given from the Razor Pages
        };

        var authorizedClaimTypes = ClaimHelper.MapToClaims(arguments.Scope).ToList();
        var userClaims = await _userClaimService.GetClaims(query.PublicSubjectId, cancellationToken);
        foreach (var userClaim in userClaims)
        {
            if (authorizedClaimTypes.Contains(userClaim.Type))
            {
                claims.Add(userClaim.Type, userClaim.Value);
            }
        }

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(query.SigningAlg);
        var signingCredentials = new SigningCredentials(signingKey, query.SigningAlg.GetDescription());

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddHours(1),
            NotBefore = now,
            Issuer = _discoveryDocumentOptions.Value.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = TokenTypeHeaderConstants.IdToken,
            Claims = claims
        };

        if (query.EncryptionAlg is not null &&
            query.EncryptionEnc is not null)
        {
            tokenDescriptor.EncryptingCredentials = await _tokenSecurityService.GetEncryptingCredentials(
            query.ClientId,
                query.EncryptionAlg.Value,
                query.EncryptionEnc.Value,
                cancellationToken);
        }

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}