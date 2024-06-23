using AuthServer.Core.RequestProcessing;
using AuthServer.RequestAccessors.Token;
using AuthServer.Core.Abstractions;

namespace AuthServer.TokenByGrant.ClientCredentialsGrant;
internal class ClientCredentialsRequestProcessor : RequestProcessor<TokenRequest, ClientCredentialsValidatedRequest, TokenResponse>
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest> _requestValidator;
    private readonly IClientCredentialsProcessor _clientCredentialsProcessor;

    public ClientCredentialsRequestProcessor(
        IUnitOfWork unitOfWork,
        IRequestValidator<TokenRequest, ClientCredentialsValidatedRequest> requestValidator,
        IClientCredentialsProcessor clientCredentialsProcessor)
    {
	    _unitOfWork = unitOfWork;
	    _requestValidator = requestValidator;
        _clientCredentialsProcessor = clientCredentialsProcessor;
    }

    protected override async Task<ProcessResult<TokenResponse, ProcessError>> ProcessRequest(ClientCredentialsValidatedRequest request, CancellationToken cancellationToken)
    {
	    using var transaction = _unitOfWork.Begin();
        var result = await _clientCredentialsProcessor.Process(request, cancellationToken);
        await _unitOfWork.Commit();
        return result;
    }

    protected override async Task<ProcessResult<ClientCredentialsValidatedRequest, ProcessError>> ValidateRequest(TokenRequest request, CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}