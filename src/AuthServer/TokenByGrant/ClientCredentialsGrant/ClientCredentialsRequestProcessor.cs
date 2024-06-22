using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;
using System.Transactions;

namespace AuthServer.TokenByGrant.ClientCredentialsGrant;
internal class ClientCredentialsRequestProcessor : RequestProcessor<TokenRequest, ClientCredentialsValidatedRequest, TokenResponse>
{
    private readonly IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest> _requestValidator;
    private readonly IClientCredentialsProcessor _clientCredentialsProcessor;

    public ClientCredentialsRequestProcessor(
        IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest> requestValidator,
        IClientCredentialsProcessor clientCredentialsProcessor)
    {
        _requestValidator = requestValidator;
        _clientCredentialsProcessor = clientCredentialsProcessor;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(ClientCredentialsValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _clientCredentialsProcessor.Process(request, cancellationToken);
        transactionScope.Complete();
        return result;
    }

    protected override async Task<ProcessResult<ClientCredentialsValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}