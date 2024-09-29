using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Cache.Entities;
using AuthServer.Constants;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorize;

internal class AuthorizeRequestValidator : BaseAuthorizeValidator, IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly IAuthorizeInteractionService _authorizeInteractionService;
    private readonly IAuthorizeRequestParameterService _authorizeRequestParameterService;

    public AuthorizeRequestValidator(
        ICachedClientStore cachedClientStore,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        IAuthorizeInteractionService authorizeInteractionService,
        IAuthorizeRequestParameterService authorizeRequestParameterService,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions,
        INonceRepository nonceRepository) : base(nonceRepository, tokenDecoder, discoveryDocumentOptions)
    {
        _cachedClientStore = cachedClientStore;
        _authorizeInteractionService = authorizeInteractionService;
        _authorizeRequestParameterService = authorizeRequestParameterService;
    }

    public async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> Validate(AuthorizeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ClientId))
        {
            return AuthorizeError.InvalidClient;
        }

        var cachedClient = await _cachedClientStore.TryGet(request.ClientId, cancellationToken);
        if (cachedClient == null)
        {
            return AuthorizeError.InvalidClient;
        }

        var isRequestObjectEmpty = string.IsNullOrEmpty(request.RequestObject);
        var isRequestUriEmpty = string.IsNullOrEmpty(request.RequestUri);
        if (!isRequestObjectEmpty && !isRequestUriEmpty)
        {
            return AuthorizeError.InvalidRequestAndRequestUri;
        }
        else if (isRequestUriEmpty && isRequestObjectEmpty && cachedClient.RequireSignedRequestObject)
        {
            return AuthorizeError.RequestOrRequestUriRequiredAsRequestObject;
        }
        else if (isRequestUriEmpty && cachedClient.RequirePushedAuthorizationRequests)
        {
            return AuthorizeError.RequestUriRequiredAsPushedAuthorizationRequest;
        }
        else if (!isRequestUriEmpty)
        {
            if (request.RequestUri!.StartsWith(RequestUriConstants.RequestUriPrefix))
            {
                var authorizeDto = await _authorizeRequestParameterService.GetRequestByPushedRequest(request.RequestUri, request.ClientId, cancellationToken);
                if (authorizeDto is null)
                {
                    return AuthorizeError.InvalidOrExpiredRequestUri;
                }

                request = new AuthorizeRequest
                {
                    IdTokenHint = authorizeDto.IdTokenHint,
                    LoginHint = authorizeDto.LoginHint,
                    Prompt = authorizeDto.Prompt,
                    Display = authorizeDto.Display,
                    ClientId = authorizeDto.ClientId,
                    RedirectUri = authorizeDto.RedirectUri,
                    CodeChallenge = authorizeDto.CodeChallenge,
                    CodeChallengeMethod = authorizeDto.CodeChallengeMethod,
                    ResponseType = authorizeDto.ResponseType,
                    Nonce = authorizeDto.Nonce,
                    MaxAge = authorizeDto.MaxAge,
                    State = authorizeDto.State,
                    ResponseMode = authorizeDto.ResponseMode,
                    RequestObject = string.Empty,
                    RequestUri = request.RequestUri,
                    Scope = authorizeDto.Scope,
                    AcrValues = authorizeDto.AcrValues
                };
                return await ValidateForInteraction(request, cachedClient, cancellationToken);
            }

            if (!Uri.TryCreate(request.RequestUri, UriKind.Absolute, out var requestUri))
            {
                return AuthorizeError.InvalidRequestUri;
            }

            if (!cachedClient.RequestUris.Contains(requestUri.GetLeftPart(UriPartial.Path)))
            {
                return AuthorizeError.UnauthorizedRequestUri;
            }

            var newRequest = await _authorizeRequestParameterService.GetRequestByReference(requestUri, request.ClientId, ClientTokenAudience.AuthorizeEndpoint, cancellationToken);
            if (newRequest is null)
            {
                return AuthorizeError.InvalidRequestFromRequestUri;
            }

            request = new AuthorizeRequest
            {
                IdTokenHint = newRequest.IdTokenHint,
                LoginHint = newRequest.LoginHint,
                Prompt = newRequest.Prompt,
                Display = newRequest.Display,
                ClientId = newRequest.ClientId,
                RedirectUri = newRequest.RedirectUri,
                CodeChallenge = newRequest.CodeChallenge,
                CodeChallengeMethod = newRequest.CodeChallengeMethod,
                ResponseType = newRequest.ResponseType,
                Nonce = newRequest.Nonce,
                MaxAge = newRequest.MaxAge,
                State = newRequest.State,
                ResponseMode = newRequest.ResponseMode,
                RequestObject = string.Empty,
                RequestUri = string.Empty,
                Scope = newRequest.Scope,
                AcrValues = newRequest.AcrValues
            };
        }
        else if (!isRequestObjectEmpty)
        {
            var newRequest = await _authorizeRequestParameterService.GetRequestByObject(request.RequestObject!, request.ClientId, ClientTokenAudience.AuthorizeEndpoint, cancellationToken);
            if (newRequest is null)
            {
                return AuthorizeError.InvalidRequest;
            }

            request = new AuthorizeRequest
            {
                IdTokenHint = newRequest.IdTokenHint,
                LoginHint = newRequest.LoginHint,
                Prompt = newRequest.Prompt,
                Display = newRequest.Display,
                ClientId = newRequest.ClientId,
                RedirectUri = newRequest.RedirectUri,
                CodeChallenge = newRequest.CodeChallenge,
                CodeChallengeMethod = newRequest.CodeChallengeMethod,
                ResponseType = newRequest.ResponseType,
                Nonce = newRequest.Nonce,
                MaxAge = newRequest.MaxAge,
                State = newRequest.State,
                ResponseMode = newRequest.ResponseMode,
                RequestObject = string.Empty,
                RequestUri = string.Empty,
                Scope = newRequest.Scope,
                AcrValues = newRequest.AcrValues
            };
        }

        if (!HasValidState(request.State))
        {
            return AuthorizeError.InvalidState;
        }

        if (!HasValidEmptyRedirectUri(request.RedirectUri, cachedClient))
        {
            return AuthorizeError.InvalidRedirectUri;
        }

        if (!HasValidRedirectUri(request.RedirectUri, cachedClient))
        {
            return AuthorizeError.UnauthorizedRedirectUri;
        }

        if (!HasValidResponseMode(request.ResponseMode))
        {
            return AuthorizeError.InvalidResponseMode;
        }

        if (!HasValidResponseType(request.ResponseType))
        {
            return AuthorizeError.InvalidResponseType;
        }

        if (!HasValidGrantType(cachedClient))
        {
            return AuthorizeError.UnauthorizedResponseType;
        }

        if (!HasValidDisplay(request.Display))
        {
            return AuthorizeError.InvalidDisplay;
        }

        if (!HasValidNonce(request.Nonce))
        {
            return AuthorizeError.InvalidNonce;
        }

        if (!await HasUniqueNonce(request.Nonce!, cancellationToken))
        {
            return AuthorizeError.ReplayNonce;
        }

        if (!HasValidCodeChallengeMethod(request.CodeChallengeMethod))
        {
            return AuthorizeError.InvalidCodeChallengeMethod;
        }

        if (!HasValidCodeChallenge(request.CodeChallenge))
        {
            return AuthorizeError.InvalidCodeChallenge;
        }

        if (!HasValidScope(request.Scope))
        {
            return AuthorizeError.InvalidOpenIdScope;
        }

        if (!HasAuthorizedScope(request.Scope, cachedClient))
        {
            return AuthorizeError.UnauthorizedScope;
        }

        if (!HasValidMaxAge(request.MaxAge))
        {
            return AuthorizeError.InvalidMaxAge;
        }

        if (!await HasValidIdTokenHint(request.IdTokenHint, request.ClientId, cancellationToken))
        {
            return AuthorizeError.InvalidIdTokenHint;
        }

        if (!HasValidPrompt(request.Prompt))
        {
            return AuthorizeError.InvalidPrompt;
        }

        if (!HasValidAcrValues(request.AcrValues))
        {
            return AuthorizeError.InvalidAcrValues;
        }

        return await ValidateForInteraction(request, cachedClient, cancellationToken);
    }

    // This must first be deduced after successful validation of all input from the request
    private async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> ValidateForInteraction(AuthorizeRequest request, CachedClient cachedClient, CancellationToken cancellationToken)
    {
        var deducedPrompt = await _authorizeInteractionService.GetPrompt(request, cancellationToken);
        if (deducedPrompt == PromptConstants.Login)
        {
            return AuthorizeError.LoginRequired;
        }

        if (deducedPrompt == PromptConstants.Consent)
        {
            return AuthorizeError.ConsentRequired;
        }

        if (deducedPrompt == PromptConstants.SelectAccount)
        {
            return AuthorizeError.AccountSelectionRequired;
        }

        var redirectUri = string.IsNullOrEmpty(request.RedirectUri)
            ? cachedClient.RedirectUris.Single()
            : request.RedirectUri;

        return new AuthorizeValidatedRequest
        {
            ResponseMode = request.ResponseMode,
            CodeChallenge = request.CodeChallenge!,
            Scope = request.Scope,
            AcrValues = request.AcrValues,
            ClientId = request.ClientId!,
            Nonce = request.Nonce!,
            RedirectUri = redirectUri,
            RequestUri = request.RequestUri
        };
    }
}