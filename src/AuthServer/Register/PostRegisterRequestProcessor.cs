using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register;

internal class PostRegisterRequestProcessor : RequestProcessor<RegisterRequest, PostRegisterValidatedRequest, PostRegisterResponse>
{
	private readonly IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> _requestValidator;

	public PostRegisterRequestProcessor(
		IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> requestValidator)
	{
		_requestValidator = requestValidator;
	}

	protected override Task<ProcessResult<PostRegisterResponse, ProcessError>> ProcessRequest(PostRegisterValidatedRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	protected override async Task<ProcessResult<PostRegisterValidatedRequest, ProcessError>> ValidateRequest(RegisterRequest request, CancellationToken cancellationToken)
	{
		return await _requestValidator.Validate(request, cancellationToken);
	}
}