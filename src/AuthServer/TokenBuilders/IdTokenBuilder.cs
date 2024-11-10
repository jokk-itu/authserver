using System.Diagnostics;
using System.Text.Json;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Metrics;
using AuthServer.Metrics.Abstractions;
using AuthServer.Options;
using AuthServer.Repositories.Abstractions;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenBuilders;

internal class IdTokenBuilder : ITokenBuilder<IdTokenArguments>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;
    private readonly IOptionsSnapshot<JwksDocument> _jwksDocumentOptions;
    private readonly ITokenSecurityService _tokenSecurityService;
    private readonly IUserClaimService _userClaimService;
    private readonly IMetricService _metricService;
    private readonly IConsentGrantRepository _consentGrantRepository;

    public IdTokenBuilder(
        AuthorizationDbContext identityContext,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        IOptionsSnapshot<JwksDocument> jwksDocumentOptions,
        ITokenSecurityService tokenSecurityService,
        IUserClaimService userClaimService,
        IMetricService metricService,
        IConsentGrantRepository consentGrantRepository)
    {
        _identityContext = identityContext;
        _discoveryDocumentOptions = discoveryDocumentOptions;
        _jwksDocumentOptions = jwksDocumentOptions;
        _tokenSecurityService = tokenSecurityService;
        _userClaimService = userClaimService;
        _metricService = metricService;
        _consentGrantRepository = consentGrantRepository;
    }

    public async Task<string> BuildToken(IdTokenArguments arguments, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        var query = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == arguments.AuthorizationGrantId)
            .Select(x => new
            {
                x.AuthTime,
                ClientId = x.Client.Id,
                SessionId = x.Session.Id,
                SubjectIdentifier = x.Session.SubjectIdentifier.Id,
                GrantSubject = x.Subject,
                SigningAlg = x.Client.IdTokenSignedResponseAlg,
                EncryptionAlg = x.Client.IdTokenEncryptedResponseAlg,
                EncryptionEnc = x.Client.IdTokenEncryptedResponseEnc,
                Nonce = x.Nonces.OrderByDescending(y => y.IssuedAt).First(),
                AuthenticationMethodReferences = x.AuthenticationMethodReferences.Select(amr => amr.Name).ToList(),
                AuthenticationContextReference = x.AuthenticationContextReference.Name
            })
            .SingleAsync(cancellationToken);

        var claims = new Dictionary<string, object>
        {
            { ClaimNameConstants.Sub, query.GrantSubject },
            { ClaimNameConstants.Aud, query.ClientId },
            { ClaimNameConstants.Sid, query.SessionId },
            { ClaimNameConstants.Jti, Guid.NewGuid() },
            { ClaimNameConstants.GrantId, arguments.AuthorizationGrantId },
            { ClaimNameConstants.Nonce, query.Nonce.Value },
            { ClaimNameConstants.ClientId, query.ClientId },
            { ClaimNameConstants.Azp, query.ClientId },
            { ClaimNameConstants.AuthTime, query.AuthTime },
            { ClaimNameConstants.Amr, JsonSerializer.SerializeToElement(query.AuthenticationMethodReferences) },
            { ClaimNameConstants.Acr, query.AuthenticationContextReference }
        };

        var authorizedClaimTypes = await _consentGrantRepository.GetConsentedClaims(query.SubjectIdentifier, query.ClientId, cancellationToken);
        var userClaims = await _userClaimService.GetClaims(query.SubjectIdentifier, cancellationToken);
        foreach (var userClaim in userClaims)
        {
            if (authorizedClaimTypes.Contains(userClaim.Type))
            {
                claims.Add(userClaim.Type, userClaim.Value);
            }
        }

        var now = DateTime.UtcNow;
        var signingKey = _jwksDocumentOptions.Value.GetSigningKey(query.SigningAlg!.Value);
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
        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        stopWatch.Stop();
        _metricService.AddBuiltToken(stopWatch.ElapsedMilliseconds, TokenTypeTag.IdToken, TokenStructureTag.Jwt);
        return jwt;
    }
}