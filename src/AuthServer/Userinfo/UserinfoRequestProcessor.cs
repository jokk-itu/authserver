using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AuthServer.Core.Request;

namespace AuthServer.Userinfo;
internal class UserinfoRequestProcessor : IRequestProcessor<UserinfoValidatedRequest, string>
{
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenBuilder<UserinfoTokenArguments> _userinfoTokenBuilder;
    private readonly IUserClaimService _userClaimService;

    public UserinfoRequestProcessor(
        AuthorizationDbContext identityContext,
        ITokenBuilder<UserinfoTokenArguments> userinfoTokenBuilder,
        IUserClaimService userClaimService)
    {
        _identityContext = identityContext;
        _userinfoTokenBuilder = userinfoTokenBuilder;
        _userClaimService = userClaimService;
    }

    public async Task<string> Process(UserinfoValidatedRequest request, CancellationToken cancellationToken)
    {
        var query = await _identityContext
            .Set<AuthorizationGrant>()
            .Where(x => x.Id == request.AuthorizationGrantId)
            .Include(x => x.Nonces)
            .Select(x => new
            {
                ClientId = x.Client.Id,
                PublicSubjectId = x.Session.PublicSubjectIdentifier.Id,
                GrantSubjectId = x.SubjectIdentifier.Id,
                SigningAlg = x.Client.UserinfoSignedResponseAlg,
                EncryptionAlg = x.Client.UserinfoEncryptedResponseAlg,
                EncryptionEnc = x.Client.UserinfoEncryptedResponseEnc
            })
            .SingleAsync(cancellationToken);

        var claims = new Dictionary<string, object>
        {
            { Parameter.Subject, query.GrantSubjectId }
        };

        var authorizedClaimTypes = ClaimHelper.MapToClaims(request.Scope).ToList();
        var userClaims = await _userClaimService.GetClaims(query.PublicSubjectId, cancellationToken);
        foreach (var userClaim in userClaims)
        {
            if (authorizedClaimTypes.Contains(userClaim.Type))
            {
                claims.Add(userClaim.Type, userClaim.Value);
            }
        }

        var clientExpectsJwt = query.SigningAlg != null;
        
        if (clientExpectsJwt)
        {
            return await _userinfoTokenBuilder.BuildToken(new UserinfoTokenArguments
            {
                ClientId = query.ClientId,
                SigningAlg = query.SigningAlg!.Value,
                EncryptionAlg = query.EncryptionAlg,
                EncryptionEnc = query.EncryptionEnc,
                EndUserClaims = claims
            }, cancellationToken);
        }

        return JsonSerializer.Serialize(claims);
    }
}