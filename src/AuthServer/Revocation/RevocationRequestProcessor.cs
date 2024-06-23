using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Revocation;
using AuthServer.Revocation.Abstractions;

namespace AuthServer.Revocation;
internal class RevocationRequestProcessor : RequestProcessor<RevocationRequest, RevocationValidatedRequest, Unit>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly ITokenRevoker _tokenRevoker;
    private readonly IRequestValidator<RevocationRequest, RevocationValidatedRequest> _requestValidator;

    public RevocationRequestProcessor(
        IUnitOfWork unitOfWork,
        ITokenRevoker tokenRevoker,
        IRequestValidator<RevocationRequest, RevocationValidatedRequest> requestValidator)
    {
	    _unitOfWork = unitOfWork;
	    _tokenRevoker = tokenRevoker;
        _requestValidator = requestValidator;
    }
    protected override async Task<ProcessResult<Unit, ProcessError>> ProcessRequest(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
	    using var transaction = _unitOfWork.Begin();
        await _tokenRevoker.Revoke(request, cancellationToken);
        await _unitOfWork.Commit();
        return new ProcessResult<Unit, ProcessError>(Unit.Value);
    }

    protected override async Task<ProcessResult<RevocationValidatedRequest, ProcessError>> ValidateRequest(RevocationRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}