using AuthServer.Authorize.Abstract;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Authorize;
using System.Transactions;

namespace AuthServer.Authorize;

internal class AuthorizeRequestProcessor : RequestProcessor<AuthorizeRequest, AuthorizeValidatedRequest, string>
{
    private readonly IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> _requestValidator;
    private readonly IAuthorizeProcessor _authorizeProcessor;

    public AuthorizeRequestProcessor(
        IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> requestValidator,
        IAuthorizeProcessor authorizeProcessor)
    {
        _requestValidator = requestValidator;
        _authorizeProcessor = authorizeProcessor;
    }

    protected override async Task<ProcessResult<string, ProcessError>> ProcessRequest(AuthorizeValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _authorizeProcessor.Process(request, cancellationToken);
        transactionScope.Complete();
        return result;
    }

    protected override async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> ValidateRequest(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}