using AuthServer.Authorization;
using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Repositories.Abstractions;

namespace AuthServer.PushedAuthorization;
internal class PushedAuthorizationRequestProcessor : IRequestProcessor<PushedAuthorizationValidatedRequest, PushedAuthorizationResponse>
{
    private readonly IClientRepository _clientRepository;

    public PushedAuthorizationRequestProcessor(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<PushedAuthorizationResponse> Process(PushedAuthorizationValidatedRequest request, CancellationToken cancellationToken)
    {
        var authorizeDto = new AuthorizeRequestDto
        {
            LoginHint = request.LoginHint,
            IdTokenHint = request.IdTokenHint,
            Prompt = request.Prompt,
            Display = request.Display,
            ResponseType = request.ResponseType,
            ResponseMode = request.ResponseMode,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            Scope = request.Scope,
            AcrValues = request.AcrValues,
            ClientId = request.ClientId,
            MaxAge = request.MaxAge,
            Nonce = request.Nonce,
            State = request.State,
            RedirectUri = request.RedirectUri
        };
        var authorizeMessage = await _clientRepository.AddAuthorizeMessage(authorizeDto, cancellationToken);

        return new PushedAuthorizationResponse
        {
            ClientId = request.ClientId,
            RequestUri = $"{RequestUriConstants.RequestUriPrefix}{authorizeMessage.Reference}",
            ExpiresIn = authorizeMessage.Client.RequestUriExpiration!.Value
        };
    }
}
