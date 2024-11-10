using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Userinfo;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Userinfo;

internal class UserinfoRequestValidator : IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>
{
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly AuthorizationDbContext _authorizationDbContext;

    public UserinfoRequestValidator(
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        AuthorizationDbContext authorizationDbContext)
    {
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _authorizationDbContext = authorizationDbContext;
    }

    public async Task<ProcessResult<UserinfoValidatedRequest, ProcessError>> Validate(UserinfoRequest request,
        CancellationToken cancellationToken)
    {
        if (TokenHelper.IsJsonWebToken(request.AccessToken))
        {
            // only read because the token has already been validated
            var token = await _serverIssuedTokenDecoder.Read(request.AccessToken);
            var authorizationGrantId = token.GetClaim(ClaimNameConstants.GrantId).Value;
            var scope = token.GetClaim(ClaimNameConstants.Scope).Value.Split(' ');
            return new UserinfoValidatedRequest
            {
                AuthorizationGrantId = authorizationGrantId,
                Scope = scope
            };
        }
        else
        {
            var query = await _authorizationDbContext
                .Set<GrantAccessToken>()
                .Where(x => x.Reference == request.AccessToken)
                .Select(x => new
                {
                    AuthorizationGrantId = x.AuthorizationGrant.Id, x.Scope
                })
                .SingleAsync(cancellationToken);

            // must at a minimum contain openid and userinfo scope
            var scope = query.Scope!.Split(' ');

            return new UserinfoValidatedRequest
            {
                AuthorizationGrantId = query.AuthorizationGrantId,
                Scope = scope
            };
        }
    }
}