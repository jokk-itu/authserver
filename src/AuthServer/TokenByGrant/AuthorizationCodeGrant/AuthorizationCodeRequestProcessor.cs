using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;

namespace AuthServer.TokenByGrant.AuthorizationCodeGrant;
internal class AuthorizationCodeRequestProcessor : RequestProcessor<TokenRequest, AuthorizationCodeValidatedRequest, TokenResponse>
{
    private readonly IAuthorizationCodeProcessor _processor;
    private readonly IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> _validator;

    public AuthorizationCodeRequestProcessor(
        IAuthorizationCodeProcessor processor,
        IRequestValidator<TokenRequest, AuthorizationCodeValidatedRequest> validator)
    {
        _processor = processor;
        _validator = validator;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(AuthorizationCodeValidatedRequest request, CancellationToken cancellationToken)
    {
        return await _processor.Process(request, cancellationToken);
    }

    protected override async Task<ProcessResult<AuthorizationCodeValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _validator.Validate(request, cancellationToken);
    }
}