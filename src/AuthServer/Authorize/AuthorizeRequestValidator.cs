using AuthServer.Authorize.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorize;

internal class AuthorizeRequestValidator : IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IAuthorizeInteractionProcessor _authorizeInteractionProcessor;
    private readonly IAuthorizeRequestParameterProcessor _authorizeRequestParameterProcessor;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public AuthorizeRequestValidator(
        ICachedClientStore cachedClientStore,
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeInteractionProcessor authorizeInteractionProcessor,
        IAuthorizeRequestParameterProcessor authorizeRequestParameterProcessor,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _cachedClientStore = cachedClientStore;
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _authorizeInteractionProcessor = authorizeInteractionProcessor;
        _authorizeRequestParameterProcessor = authorizeRequestParameterProcessor;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> Validate(AuthorizeRequest request,
        CancellationToken cancellationToken)
    {
        var cachedClient = await _cachedClientStore.TryGet(request.ClientId, cancellationToken);
        if (cachedClient == null)
        {
            return AuthorizeError.InvalidClient;
        }

        if (!string.IsNullOrEmpty(request.RequestObject) && !string.IsNullOrEmpty(request.RequestUri))
        {
            return AuthorizeError.InvalidRequestObjectAndUri;
        }
        else if (!string.IsNullOrEmpty(request.RequestUri))
        {
            if (!Uri.TryCreate(request.RequestUri, UriKind.Absolute, out var requestUri))
            {
                return AuthorizeError.InvalidRequestUri;
            }

            if (!cachedClient.RequestUris.Contains(requestUri.GetLeftPart(UriPartial.Path)))
            {
                return AuthorizeError.UnauthorizedRequestUri;
            }

            var newRequest = await _authorizeRequestParameterProcessor.GetRequestByReference(requestUri, request.ClientId, cancellationToken);
            if (newRequest is null)
            {
                return AuthorizeError.InvalidObjectFromRequestUri;
            }
        }
        else if (!string.IsNullOrEmpty(request.RequestObject))
        {
            var newRequest = await _authorizeRequestParameterProcessor.GetRequestByObject(request.RequestObject, request.ClientId, cancellationToken);
            if (newRequest is null)
            {
                return AuthorizeError.InvalidRequestObject;
            }
        }

        if (string.IsNullOrEmpty(request.State))
        {
            return AuthorizeError.InvalidState;
        }

        if (string.IsNullOrEmpty(request.RedirectUri)
            && cachedClient.RedirectUris.Count != 1)
        {
            return AuthorizeError.InvalidRedirectUri;
        }

        if (!string.IsNullOrEmpty(request.RedirectUri)
            && cachedClient.RedirectUris.Any(x => x == request.RedirectUri))
        {
            return AuthorizeError.UnauthorizedRedirectUri;
        }

        if (!string.IsNullOrEmpty(request.ResponseMode)
            && !ResponseModeConstants.ResponseModes.Contains(request.ResponseMode))
        {
            return AuthorizeError.InvalidResponseMode;
        }

        // it is currently only code which is supported
        if (!ResponseTypeConstants.ResponseTypes.Contains(request.ResponseType))
        {
            return AuthorizeError.InvalidResponseType;
        }

        // it is currently only code which is supported
        if (cachedClient.GrantTypes.All(x => x != GrantTypeConstants.AuthorizationCode))
        {
            return AuthorizeError.UnauthorizedResponseType;
        }

        if (!string.IsNullOrEmpty(request.Display)
            && !DisplayConstants.DisplayValues.Contains(request.Display))
        {
            return AuthorizeError.InvalidDisplay;
        }

        if (string.IsNullOrEmpty(request.Nonce))
        {
            return AuthorizeError.InvalidNonce;
        }

        var nonceExists = await _identityContext
            .Set<Nonce>()
            .AnyAsync(x => x.Value == request.Nonce, cancellationToken);

        if (nonceExists)
        {
            return AuthorizeError.ReplayNonce;
        }

        if (!ProofKeyForCodeExchangeHelper.IsCodeChallengeMethodValid(request.CodeChallengeMethod))
        {
            return AuthorizeError.InvalidCodeChallengeMethod;
        }

        if (!ProofKeyForCodeExchangeHelper.IsCodeChallengeValid(request.CodeChallenge))
        {
            return AuthorizeError.InvalidCodeChallenge;
        }

        if (!request.Scope.Contains(ScopeConstants.OpenId))
        {
            return AuthorizeError.InvalidOpenIdScope;
        }

        var isClientUnauthorizedForScope = request.Scope.ExceptAny(cachedClient.Scopes);
        if (isClientUnauthorizedForScope)
        {
            return AuthorizeError.UnauthorizedScope;
        }

        if (!MaxAgeHelper.IsMaxAgeValid(request.MaxAge))
        {
            return AuthorizeError.InvalidMaxAge;
        }

        if (!string.IsNullOrEmpty(request.IdTokenHint))
        {
            try
            {
                await _serverIssuedTokenDecoder.Validate(
                    request.IdTokenHint,
                    new ServerIssuedTokenDecodeArguments
                    {
                        ValidateLifetime = true,
                        TokenTypes = [TokenTypeHeaderConstants.IdToken],
                        Audiences = [request.ClientId]
                    }, cancellationToken);
            }
            catch
            {
                return AuthorizeError.InvalidIdTokenHint;
            }
        }

        if (!string.IsNullOrEmpty(request.Prompt)
            && !PromptConstants.Prompts.Contains(request.Prompt))
        {
            return AuthorizeError.InvalidPrompt;
        }

        if (request.AcrValues.Count != 0 &&
            request.AcrValues.ExceptAny(_discoveryDocumentOptions.Value.AcrValuesSupported))
        {
            return AuthorizeError.InvalidAcr;
        }

        var deducedPrompt = await _authorizeInteractionProcessor.ProcessForInteraction(request, cancellationToken);
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

        var maxAge = string.IsNullOrEmpty(request.MaxAge)
            ? cachedClient.DefaultMaxAge?.ToString() ?? string.Empty
            : request.MaxAge;

        var redirectUri = string.IsNullOrEmpty(request.RedirectUri)
            ? cachedClient.RedirectUris.Single()
            : request.RedirectUri;

        return new AuthorizeValidatedRequest
        {
            ResponseType = request.ResponseType,
            ResponseMode = request.ResponseMode,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            Scope = request.Scope,
            AcrValues = request.AcrValues,
            ClientId = request.ClientId,
            MaxAge = maxAge,
            Nonce = request.Nonce,
            State = request.State,
            RedirectUri = redirectUri
        };
    }
}