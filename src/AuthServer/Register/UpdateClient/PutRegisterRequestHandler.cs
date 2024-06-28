using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register.UpdateClient;

internal class PutRegisterRequestHandler : RequestHandler<PutRegisterRequest, PutRegisterValidatedRequest, RegisterResponse>
{
    private readonly IRequestValidator<PutRegisterRequest, PutRegisterValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<PutRegisterValidatedRequest, RegisterResponse> _requestProcessor;
    private readonly IUnitOfWork _unitOfWork;

    public PutRegisterRequestHandler(
        IRequestValidator<PutRegisterRequest, PutRegisterValidatedRequest> requestValidator,
        IRequestProcessor<PutRegisterValidatedRequest, RegisterResponse> requestProcessor,
        IUnitOfWork unitOfWork)
    {
        _requestValidator = requestValidator;
        _requestProcessor = requestProcessor;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<ProcessResult<RegisterResponse, ProcessError>> ProcessRequest(PutRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<PutRegisterValidatedRequest, ProcessError>> ValidateRequest(PutRegisterRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}