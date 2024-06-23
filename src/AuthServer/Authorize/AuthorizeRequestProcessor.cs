using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Core.Abstractions;

namespace AuthServer.Authorize;

internal class AuthorizeRequestProcessor : RequestProcessor<AuthorizeRequest, AuthorizeValidatedRequest, string>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> _requestValidator;
    private readonly IAuthorizeProcessor _authorizeProcessor;

    public AuthorizeRequestProcessor(
        IUnitOfWork unitOfWork,
        IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest> requestValidator,
        IAuthorizeProcessor authorizeProcessor)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
        _authorizeProcessor = authorizeProcessor;
    }

    protected override async Task<ProcessResult<string, ProcessError>> ProcessRequest(AuthorizeValidatedRequest request, CancellationToken cancellationToken)
    {
	    using var transaction = _unitOfWork.Begin();
        var result = await _authorizeProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> ValidateRequest(AuthorizeRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}