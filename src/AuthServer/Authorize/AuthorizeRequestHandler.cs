using AuthServer.RequestAccessors.Authorize;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;

namespace AuthServer.Authorize;

internal class AuthorizeRequestHandler : RequestHandler<AuthorizeRequest, AuthorizeValidatedRequest, string>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<AuthorizeValidatedRequest, string> _requestProcessor;

    public AuthorizeRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> requestValidator,
        IRequestProcessor<AuthorizeValidatedRequest, string> requestProcessor,
        IMetricService metricService)
        : base(metricService)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
	    _requestProcessor = requestProcessor;
    }

    protected override async Task<ProcessResult<string, ProcessError>> ProcessRequest(AuthorizeValidatedRequest request, CancellationToken cancellationToken)
    {
	    await _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return result;
    }

    protected override async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> ValidateRequest(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}