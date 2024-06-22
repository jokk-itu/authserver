using System.Transactions;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;
internal class AuthorizationCodeRequestProcessor : RequestProcessor<TokenRequest, AuthorizationCodeValidatedRequest, TokenResponse>
{
    private readonly IAuthorizationCodeProcessor _processor;
    private readonly IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> _validator;

    public AuthorizationCodeRequestProcessor(
        IAuthorizationCodeProcessor processor,
        IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> validator)
    {
        _processor = processor;
        _validator = validator;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(AuthorizationCodeValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _processor.Process(request, cancellationToken);
        transactionScope.Complete();
        return result;
    }

    protected override async Task<ProcessResult<AuthorizationCodeValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _validator.Validate(request, cancellationToken);
    }
}