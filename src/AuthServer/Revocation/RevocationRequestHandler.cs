using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;
using AuthServer.RequestAccessors.Revocation;

namespace AuthServer.Revocation;
internal class RevocationRequestHandler : RequestHandler<RevocationRequest, RevocationValidatedRequest, Unit>
{
	private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestValidator<RevocationRequest, RevocationValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<RevocationValidatedRequest, Unit> _requestProcessor;

    public RevocationRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<RevocationRequest, RevocationValidatedRequest> requestValidator,
        IRequestProcessor<RevocationValidatedRequest, Unit> requestProcessor,
        IMetricService metricService)
        : base(metricService)
    {
	    _unitOfWork = unitOfWork;
        _requestValidator = requestValidator;
        _requestProcessor = requestProcessor;
    }
    protected override async Task<ProcessResult<Unit, ProcessError>> ProcessRequest(RevocationValidatedRequest request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Begin();
        await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return new ProcessResult<Unit, ProcessError>(Unit.Value);
    }

    protected override async Task<ProcessResult<RevocationValidatedRequest, ProcessError>> ValidateRequest(RevocationRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}