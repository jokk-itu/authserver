using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;
using AuthServer.RequestAccessors.EndSession;

namespace AuthServer.EndSession;
internal class EndSessionRequestHandler : RequestHandler<EndSessionRequest, EndSessionValidatedRequest, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestValidator<EndSessionRequest, EndSessionValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<EndSessionValidatedRequest, Unit> _requestProcessor;

    public EndSessionRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<EndSessionRequest, EndSessionValidatedRequest> requestValidator,
        IRequestProcessor<EndSessionValidatedRequest, Unit> requestProcessor,
        IMetricService metricService)
        : base(metricService)
    {
        _unitOfWork = unitOfWork;
        _requestValidator = requestValidator;
        _requestProcessor = requestProcessor;
    }

    protected override async Task<ProcessResult<Unit, ProcessError>> ProcessRequest(EndSessionValidatedRequest request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return result;
    }

    protected override async Task<ProcessResult<EndSessionValidatedRequest, ProcessError>> ValidateRequest(EndSessionRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}
