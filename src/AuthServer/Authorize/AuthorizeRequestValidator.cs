using AuthServer.Authorize.Abstract;
using AuthServer.Cache.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.RequestProcessing;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Authorize;

internal class AuthorizeRequestValidator : IRequestValidator<AuthorizeRequest, AuthorizeValidatedRequest>
{
    private readonly ICachedClientStore _cachedClientStore;
    private readonly AuthorizationDbContext _identityContext;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _serverIssuedTokenDecoder;
    private readonly IAuthorizeInteractionProcessor _authorizeInteractionProcessor;

    public AuthorizeRequestValidator(
        ICachedClientStore cachedClientStore,
        AuthorizationDbContext identityContext,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> serverIssuedTokenDecoder,
        IAuthorizeInteractionProcessor authorizeInteractionProcessor)
    {
        _cachedClientStore = cachedClientStore;
        _identityContext = identityContext;
        _serverIssuedTokenDecoder = serverIssuedTokenDecoder;
        _authorizeInteractionProcessor = authorizeInteractionProcessor;
    }

    public async Task<ProcessResult<AuthorizeValidatedRequest, ProcessError>> Validate(AuthorizeRequest request,
        CancellationToken cancellationToken)
    {
        // TODO if request_object or request_uri is present, use make a new AuthorizeRequest from that in a new service

        if (string.IsNullOrEmpty(request.State))
        {
            return AuthorizeError.InvalidState;
        }

        var cachedClient = await _cachedClientStore.TryGet(request.ClientId, cancellationToken);
        if (cachedClient == null)
        {
            return AuthorizeError.InvalidClient;
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
        if (cachedClient.GrantTypes.Any(x => x == GrantTypeConstants.AuthorizationCode))
        {
            return AuthorizeError.UnauthorizedResponseType;
        }

        if (!string.IsNullOrEmpty(request.Display)
            && !DisplayConstants.DisplayValues.Contains(request.Display))
        {
            return AuthorizeError.InvalidDisplay;
        }

        if (!string.IsNullOrEmpty(request.Nonce))
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

        var isClientUnauthorizedForScope = cachedClient.Scopes.Except(request.Scope).Any();
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

        if (AcrValueConstants.AcrValues.Except(request.AcrValues).Any())
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