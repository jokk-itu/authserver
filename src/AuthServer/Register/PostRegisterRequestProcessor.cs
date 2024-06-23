using AuthServer.Core.RequestProcessing;
using AuthServer.Register.Abstractions;
using AuthServer.RequestAccessors.Register;
using AuthServer.Core.Abstractions;

namespace AuthServer.Register;

internal class PostRegisterRequestProcessor : RequestProcessor<RegisterRequest, PostRegisterValidatedRequest, PostRegisterResponse>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> _requestValidator;
    private readonly IPostRegisterProcessor _postRegisterProcessor;

    public PostRegisterRequestProcessor(
        IUnitOfWork unitOfWork,
		IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> requestValidator,
		IPostRegisterProcessor postRegisterProcessor)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
        _postRegisterProcessor = postRegisterProcessor;
    }

	protected override async Task<ProcessResult<PostRegisterResponse, ProcessError>> ProcessRequest(PostRegisterValidatedRequest request, CancellationToken cancellationToken)
	{
		using var transaction = _unitOfWork.Begin();
        var result = await _postRegisterProcessor.Register(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

	protected override async Task<ProcessResult<PostRegisterValidatedRequest, ProcessError>> ValidateRequest(RegisterRequest request, CancellationToken cancellationToken)
	{
		return await _requestValidator.Validate(request, cancellationToken);
	}
}