using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;
using AuthServer.RequestAccessors.PushedAuthorization;

namespace AuthServer.PushedAuthorization;

internal class PushedAuthorizationRequestHandler : RequestHandler<PushedAuthorizationRequest,
    PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>
{
    private readonly IRequestValidator<PushedAuthorizationRequest, PushedAuthorizationValidatedRequest>
        _requestValidator;

    private readonly IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>
        _requestProcessor;

    private readonly IUnitOfWork _unitOfWork;

    public PushedAuthorizationRequestHandler(
        IMetricService metricService,
        IRequestValidator<PushedAuthorizationRequest, PushedAuthorizationValidatedRequest> requestValidator,
        IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse> requestProcessor,
        IUnitOfWork unitOfWork)
        : base(metricService)
    {
        _requestValidator = requestValidator;
        _requestProcessor = requestProcessor;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<ProcessResult<PushedAuthorizationResponse, ProcessError>> ProcessRequest(
        PushedAuthorizationValidatedRequest request, CancellationToken cancellationToken)
    {
        await _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return result;
    }

    protected override async Task<ProcessResult<PushedAuthorizationValidatedRequest, ProcessError>> ValidateRequest(
        PushedAuthorizationRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}