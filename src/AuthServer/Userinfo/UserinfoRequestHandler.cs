using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;
using AuthServer.RequestAccessors.Userinfo;

namespace AuthServer.Userinfo;
internal class UserinfoRequestHandler : RequestHandler<UserinfoRequest, UserinfoValidatedRequest, string>
{
    private readonly IRequestProcessor<UserinfoValidatedRequest, string> _userinfoProcessor;
    private readonly IRequestValidator<UserinfoRequest, UserinfoValidatedRequest> _requestValidator;

    public UserinfoRequestHandler(
	    IRequestProcessor<UserinfoValidatedRequest, string> userinfoProcessor,
        IRequestValidator<UserinfoRequest, UserinfoValidatedRequest> requestValidator,
        IMetricService metricService)
        : base(metricService)
    {
        _userinfoProcessor = userinfoProcessor;
        _requestValidator = requestValidator;
    }

    protected override async Task<ProcessResult<string, ProcessError>> ProcessRequest(UserinfoValidatedRequest request, CancellationToken cancellationToken)
    {
        return await _userinfoProcessor.Process(request, cancellationToken);
    }

    protected override async Task<ProcessResult<UserinfoValidatedRequest, ProcessError>> ValidateRequest(UserinfoRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}