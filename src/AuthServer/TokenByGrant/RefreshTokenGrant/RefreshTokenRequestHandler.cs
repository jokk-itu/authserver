using AuthServer.RequestAccessors.Token;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Metrics.Abstractions;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;
internal class RefreshTokenRequestHandler : RequestHandler<TokenRequest, RefreshTokenValidatedRequest, TokenResponse>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> _requestValidator;
    private readonly IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse> _refreshTokenProcessor;

    public RefreshTokenRequestHandler(
        IUnitOfWork unitOfWork,
        IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> requestValidator,
        IRequestProcessor<RefreshTokenValidatedRequest, TokenResponse> refreshTokenProcessor,
        IMetricService metricService)
        : base(metricService)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
        _refreshTokenProcessor = refreshTokenProcessor;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(RefreshTokenValidatedRequest request, CancellationToken cancellationToken)
    {
	    await _unitOfWork.Begin();
        var result = await _refreshTokenProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);
        return result;
    }

    protected override async Task<ProcessResult<RefreshTokenValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}