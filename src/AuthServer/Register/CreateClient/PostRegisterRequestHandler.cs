using AuthServer.RequestAccessors.Register;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;

namespace AuthServer.Register.CreateClient;

internal class PostRegisterRequestHandler : RequestHandler<PostRegisterRequest, PostRegisterValidatedRequest, RegisterResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestValidator<PostRegisterRequest, PostRegisterValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<PostRegisterValidatedRequest, RegisterResponse> _requestProcessor;

    public PostRegisterRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<PostRegisterRequest, PostRegisterValidatedRequest> requestValidator,
        IRequestProcessor<PostRegisterValidatedRequest, RegisterResponse> registerProcessor)
    {
        _unitOfWork = unitOfWork;
        _requestValidator = requestValidator;
        _requestProcessor = registerProcessor;
    }

    protected override async Task<ProcessResult<RegisterResponse, ProcessError>> ProcessRequest(PostRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transaction = _unitOfWork.Begin();
        var result = await _requestProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<PostRegisterValidatedRequest, ProcessError>> ValidateRequest(PostRegisterRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}