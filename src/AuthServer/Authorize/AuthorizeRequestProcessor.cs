using AuthServer.Authorize.Abstract;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Authorize;

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
        return await _authorizeProcessor.Process(request, cancellationToken);
    }

    protected override async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> ValidateRequest(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}