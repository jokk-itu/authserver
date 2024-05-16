using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;
internal class RefreshTokenRequestProcessor : RequestProcessor<TokenRequest, RefreshTokenValidatedRequest, TokenResponse>
{
    protected override Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(RefreshTokenValidatedRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<ProcessResult<RefreshTokenValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}