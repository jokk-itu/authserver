using AuthServer.RequestAccessors.Register;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;

namespace AuthServer.Register;

internal class RegisterRequestHandler : RequestHandler<RegisterRequest, RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestValidator<RegisterRequest, RegisterValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>> _requestProcessor;

    public RegisterRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<RegisterRequest, RegisterValidatedRequest> requestValidator,
        IRequestProcessor<RegisterValidatedRequest, ProcessResult<RegisterResponse, Unit>> registerProcessor)
    {
        _unitOfWork = unitOfWork;
        _requestValidator = requestValidator;
        _requestProcessor = registerProcessor;
    }

    protected override async Task<ProcessResult<ProcessResult<RegisterResponse, Unit>, ProcessError>> ProcessRequest(RegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<RegisterValidatedRequest, ProcessError>> ValidateRequest(RegisterRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}