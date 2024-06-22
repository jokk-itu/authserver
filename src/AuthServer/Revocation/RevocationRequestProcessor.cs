using System.Transactions;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Revocation;
using AuthServer.Revocation.Abstractions;

namespace AuthServer.Revocation;
internal class RevocationRequestProcessor : RequestProcessor<RevocationRequest, RevocationValidatedRequest, Unit>
{
    private readonly ITokenRevoker _tokenRevoker;
    private readonly IRequestValidator<RevocationRequest, RevocationValidatedRequest> _requestValidator;

    public RevocationRequestProcessor(
        ITokenRevoker tokenRevoker,
        IRequestValidator<RevocationRequest, RevocationValidatedRequest> requestValidator)
    {
        _tokenRevoker = tokenRevoker;
        _requestValidator = requestValidator;
    }
    protected override async Task<ProcessResult<Unit, ProcessError>> ProcessRequest(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        await _tokenRevoker.Revoke(request, cancellationToken);
        transactionScope.Complete();
        return new ProcessResult<Unit, ProcessError>(Unit.Value);
    }

    protected override async Task<ProcessResult<RevocationValidatedRequest, ProcessError>> ValidateRequest(RevocationRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}