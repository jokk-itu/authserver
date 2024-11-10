using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Authorization;
using AuthServer.Authorization.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Options;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.PushedAuthorization;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthServer.PushedAuthorization;
internal class PushedAuthorizationRequestValidator : BaseAuthorizeValidator, IRequestValidator<PushedAuthorizationRequest, PushedAuthorizationValidatedRequest>
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ISecureRequestService _secureRequestService;

    public PushedAuthorizationRequestValidator(
        ICachedClientStore cachedClientStore,
        IClientAuthenticationService clientAuthenticationService,
        INonceRepository nonceRepository,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        ISecureRequestService secureRequestService)
        : base(nonceRepository, tokenDecoder, discoveryDocumentOptions)
    {
        _cachedClientStore = cachedClientStore;
        _clientAuthenticationService = clientAuthenticationService;
        _secureRequestService = secureRequestService;
    }

    public async Task<ProcessResult<PushedAuthorizationValidatedRequest, ProcessError>> Validate(PushedAuthorizationRequest request, CancellationToken cancellationToken)
    {
        var isClientAuthenticationMethodInvalid = request.ClientAuthentications.Count != 1;
        if (isClientAuthenticationMethodInvalid)
        {
            return PushedAuthorizationError.MultipleOrNoneClientMethod;
        }

        var clientAuthentication = request.ClientAuthentications.Single();
        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateClient(clientAuthentication, cancellationToken);
        if (!clientAuthenticationResult.IsAuthenticated || string.IsNullOrWhiteSpace(clientAuthenticationResult.ClientId))
        {
            return PushedAuthorizationError.InvalidClient;
        }

        var cachedClient = await _cachedClientStore.Get(clientAuthenticationResult.ClientId, cancellationToken);
        var isRequestObjectEmpty = string.IsNullOrEmpty(request.RequestObject);
        if (isRequestObjectEmpty && cachedClient.RequireSignedRequestObject)
        {
            return PushedAuthorizationError.RequestRequiredAsRequestObject;
        }
        else if (!isRequestObjectEmpty)
        {
            var newRequest = await _secureRequestService.GetRequestByObject(request.RequestObject!, clientAuthenticationResult.ClientId, ClientTokenAudience.PushedAuthorizeEndpoint, cancellationToken);
            if (newRequest is null)
            {
                return PushedAuthorizationError.InvalidRequest;
            }

            request = new PushedAuthorizationRequest
            {
                IdTokenHint = newRequest.IdTokenHint,
                LoginHint = newRequest.LoginHint,
                Prompt = newRequest.Prompt,
                Display = newRequest.Display,
                RedirectUri = newRequest.RedirectUri,
                CodeChallenge = newRequest.CodeChallenge,
                CodeChallengeMethod = newRequest.CodeChallengeMethod,
                ResponseType = newRequest.ResponseType,
                Nonce = newRequest.Nonce,
                MaxAge = newRequest.MaxAge,
                State = newRequest.State,
                ResponseMode = newRequest.ResponseMode,
                RequestObject = string.Empty,
                Scope = newRequest.Scope,
                AcrValues = newRequest.AcrValues,
                ClientAuthentications = request.ClientAuthentications
            };
        }

        if (!HasValidState(request.State))
        {
            return PushedAuthorizationError.InvalidState;
        }

        if (!HasValidEmptyRedirectUri(request.RedirectUri, cachedClient))
        {
            return PushedAuthorizationError.InvalidRedirectUri;
        }

        if (!HasValidRedirectUri(request.RedirectUri, cachedClient))
        {
            return PushedAuthorizationError.UnauthorizedRedirectUri;
        }

        if (!HasValidResponseMode(request.ResponseMode))
        {
            return PushedAuthorizationError.InvalidResponseMode;
        }

        if (!HasValidResponseType(request.ResponseType))
        {
            return PushedAuthorizationError.InvalidResponseType;
        }

        if (!HasValidGrantType(cachedClient))
        {
            return PushedAuthorizationError.UnauthorizedResponseType;
        }

        if (!HasValidDisplay(request.Display))
        {
            return PushedAuthorizationError.InvalidDisplay;
        }

        if (!HasValidNonce(request.Nonce))
        {
            return PushedAuthorizationError.InvalidNonce;
        }

        if (!await HasUniqueNonce(request.Nonce!, cancellationToken))
        {
            return PushedAuthorizationError.ReplayNonce;
        }

        if (!HasValidCodeChallengeMethod(request.CodeChallengeMethod))
        {
            return PushedAuthorizationError.InvalidCodeChallengeMethod;
        }

        if (!HasValidCodeChallenge(request.CodeChallenge))
        {
            return PushedAuthorizationError.InvalidCodeChallenge;
        }

        if (!HasValidScope(request.Scope))
        {
            return PushedAuthorizationError.InvalidOpenIdScope;
        }

        if (!HasAuthorizedScope(request.Scope, cachedClient))
        {
            return PushedAuthorizationError.UnauthorizedScope;
        }

        if (!HasValidMaxAge(request.MaxAge))
        {
            return PushedAuthorizationError.InvalidMaxAge;
        }

        if (!await HasValidIdTokenHint(request.IdTokenHint, clientAuthenticationResult.ClientId, cancellationToken))
        {
            return PushedAuthorizationError.InvalidIdTokenHint;
        }

        if (!HasValidPrompt(request.Prompt))
        {
            return PushedAuthorizationError.InvalidPrompt;
        }

        if (!HasValidAcrValues(request.AcrValues))
        {
            return PushedAuthorizationError.InvalidAcrValues;
        }

        return new PushedAuthorizationValidatedRequest
        {
            LoginHint = request.LoginHint,
            IdTokenHint = request.IdTokenHint,
            Prompt = request.Prompt,
            Display = request.Display,
            ResponseType = request.ResponseType!,
            ResponseMode = request.ResponseMode,
            CodeChallenge = request.CodeChallenge!,
            CodeChallengeMethod = request.CodeChallengeMethod!,
            Scope = request.Scope,
            AcrValues = request.AcrValues,
            ClientId = clientAuthenticationResult.ClientId,
            MaxAge = request.MaxAge,
            Nonce = request.Nonce!,
            State = request.State!,
            RedirectUri = request.RedirectUri
        };
    }
}
