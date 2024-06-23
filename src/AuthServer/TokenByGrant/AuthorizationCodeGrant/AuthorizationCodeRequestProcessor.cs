using AuthServer.Core.Abstractions;
using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;
internal class AuthorizationCodeRequestProcessor : RequestProcessor<TokenRequest, AuthorizationCodeValidatedRequest, TokenResponse>
{
    private readonly IAuthorizationCodeProcessor _processor;
    private readonly IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizationCodeRequestProcessor(
        IAuthorizationCodeProcessor processor,
        IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> validator,
        IUnitOfWork unitOfWork)
    {
        _processor = processor;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(AuthorizationCodeValidatedRequest request, CancellationToken cancellationToken)
    {
	    using var transaction = _unitOfWork.Begin();
        var result = await _processor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<AuthorizationCodeValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _validator.Validate(request, cancellationToken);
    }
}