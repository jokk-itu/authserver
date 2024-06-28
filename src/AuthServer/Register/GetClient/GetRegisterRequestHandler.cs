using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register.GetClient;

internal class GetRegisterRequestHandler : RequestHandler<GetRegisterRequest, GetRegisterValidatedRequest, RegisterResponse>
{
	private readonly IRequestValidator<GetRegisterRequest, GetRegisterValidatedRequest> _requestValidator;
	private readonly IRequestProcessor<GetRegisterValidatedRequest, RegisterResponse> _requestProcessor;
	private readonly IUnitOfWork _unitOfWork;

	public GetRegisterRequestHandler(
		IRequestValidator<GetRegisterRequest, GetRegisterValidatedRequest> requestValidator,
		IRequestProcessor<GetRegisterValidatedRequest, RegisterResponse> requestProcessor,
		IUnitOfWork unitOfWork)
	{
		_requestValidator = requestValidator;
		_requestProcessor = requestProcessor;
		_unitOfWork = unitOfWork;
	}

	protected override async Task<ProcessResult<RegisterResponse, ProcessError>> ProcessRequest(GetRegisterValidatedRequest request, CancellationToken cancellationToken)
	{
		using var transaction = _unitOfWork.Begin();
		var result = await _requestProcessor.Process(request, cancellationToken);
		await _unitOfWork.Commit();
		return result;
	}

	protected override async Task<ProcessResult<GetRegisterValidatedRequest, ProcessError>> ValidateRequest(GetRegisterRequest request, CancellationToken cancellationToken)
	{
		return await _requestValidator.Validate(request, cancellationToken);
	}
}