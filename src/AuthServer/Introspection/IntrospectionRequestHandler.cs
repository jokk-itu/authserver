using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;
using AuthServer.RequestAccessors.Introspection;

namespace AuthServer.Introspection;

internal class
    IntrospectionRequestHandler : RequestHandler<IntrospectionRequest, IntrospectionValidatedRequest, IntrospectionResponse>
{
    private readonly IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse> _requestProcessor;
    private readonly IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest> _requestValidator;

    public IntrospectionRequestHandler(
		IRequestProcessor<IntrospectionValidatedRequest, IntrospectionResponse> requestProcessor,
		IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest> requestValidator,
        IMetricService metricService)
        : base(metricService)
    {
	    _requestProcessor = requestProcessor;
        _requestValidator = requestValidator;
    }

    protected override async Task<ProcessResult<IntrospectionResponse, ProcessError>> ProcessRequest(
        IntrospectionValidatedRequest request, CancellationToken cancellationToken)
    {
        return await _requestProcessor.Process(request, cancellationToken);
    }

    protected override async Task<ProcessResult<IntrospectionValidatedRequest, ProcessError>> ValidateRequest(IntrospectionRequest request,
        CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}