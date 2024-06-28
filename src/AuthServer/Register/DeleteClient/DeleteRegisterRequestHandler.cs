using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register.DeleteClient;

internal class DeleteRegisterRequestHandler : RequestHandler<DeleteRegisterRequest, DeleteRegisterValidatedRequest, Unit>
{
    private readonly IRequestProcessor<DeleteRegisterValidatedRequest, Unit> _requestProcessor;
    private readonly IRequestValidator<DeleteRegisterRequest, DeleteRegisterValidatedRequest> _requestValidator;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRegisterRequestHandler(
        IRequestProcessor<DeleteRegisterValidatedRequest, Unit> requestProcessor,
        IRequestValidator<DeleteRegisterRequest, DeleteRegisterValidatedRequest> requestValidator,
        IUnitOfWork unitOfWork)
    {
        _requestProcessor = requestProcessor;
        _requestValidator = requestValidator;
        _unitOfWork = unitOfWork;
    }


    protected override async Task<ProcessResult<Unit, ProcessError>> ProcessRequest(DeleteRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<DeleteRegisterValidatedRequest, ProcessError>> ValidateRequest(DeleteRegisterRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}