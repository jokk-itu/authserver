using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;
using System.Transactions;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;
internal class RefreshTokenRequestProcessor : RequestProcessor<TokenRequest, RefreshTokenValidatedRequest, TokenResponse>
{
    private readonly IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> _requestValidator;
    private readonly IRefreshTokenProcessor _refreshTokenProcessor;

    public RefreshTokenRequestProcessor(
        IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> requestValidator,
        IRefreshTokenProcessor refreshTokenProcessor)
    {
        _requestValidator = requestValidator;
        _refreshTokenProcessor = refreshTokenProcessor;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(RefreshTokenValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _refreshTokenProcessor.Process(request, cancellationToken);
        transactionScope.Complete();
        return result;
    }

    protected override async Task<ProcessResult<RefreshTokenValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}