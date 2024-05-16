using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.ClientCredentialsGrant;
internal class ClientCredentialsRequestProcessor : RequestProcessor<TokenRequest, ClientCredentialsValidatedRequest, TokenResponse>
{
    protected override Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(ClientCredentialsValidatedRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<ProcessResult<ClientCredentialsValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}