using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;
using AuthServer.Core.Abstractions;

namespace AuthServer.TokenByGrant.RefreshTokenGrant;
internal class RefreshTokenRequestProcessor : RequestProcessor<TokenRequest, RefreshTokenValidatedRequest, TokenResponse>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> _requestValidator;
    private readonly IRefreshTokenProcessor _refreshTokenProcessor;

    public RefreshTokenRequestProcessor(
        IUnitOfWork unitOfWork,
        IRequestValidator<TokenRequest, RefreshTokenValidatedRequest> requestValidator,
        IRefreshTokenProcessor refreshTokenProcessor)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
        _refreshTokenProcessor = refreshTokenProcessor;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(RefreshTokenValidatedRequest request, CancellationToken cancellationToken)
    {
	    using var transaction = _unitOfWork.Begin();
        var result = await _refreshTokenProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<RefreshTokenValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}