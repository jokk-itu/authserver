using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Userinfo;

namespace AuthServer.Userinfo;
internal class UserinfoRequestProcessor : RequestProcessor<UserinfoRequest, UserinfoValidatedRequest, string>
{
    private readonly IUserinfoProcessor _userinfoProcessor;
    private readonly IRequestValidator<UserinfoRequest, UserinfoValidatedRequest> _requestValidator;

    public UserinfoRequestProcessor(
        IUserinfoProcessor userinfoProcessor,
        IRequestValidator<UserinfoRequest, UserinfoValidatedRequest> requestValidator)
    {
        _userinfoProcessor = userinfoProcessor;
        _requestValidator = requestValidator;
    }

    protected override async Task<ProcessResult<string, ProcessError>> ProcessRequest(UserinfoValidatedRequest request, CancellationToken cancellationToken)
    {
        return await _userinfoProcessor.GetUserinfo(request, cancellationToken);
    }

    protected override async Task<ProcessResult<UserinfoValidatedRequest, ProcessError>> ValidateRequest(UserinfoRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}