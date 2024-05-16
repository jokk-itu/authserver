using AuthServer.Constants;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Userinfo;
using AuthServer.TokenDecoders;

namespace AuthServer.Userinfo;

internal class UserinfoRequestValidator : IRequestValidator<UserinfoRequest, UserinfoValidatedRequest>
{
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;

    public UserinfoRequestValidator(ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder)
    {
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
    }

    public async Task<ProcessResult<UserinfoValidatedRequest, ProcessError>> Validate(UserinfoRequest request,
        CancellationToken cancellationToken)
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
}