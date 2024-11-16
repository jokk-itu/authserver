using AuthServer.Cache.Entities;
using AuthServer.Constants;
using AuthServer.Extensions;
using AuthServer.Helpers;
using AuthServer.Options;
using AuthServer.Repositories.Abstractions;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorization;
internal class BaseAuthorizeValidator
{
    private readonly INonceRepository _nonceRepository;
    private readonly ITokenDecoder<ServerIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public BaseAuthorizeValidator(
        INonceRepository nonceRepository,
        ITokenDecoder<ServerIssuedTokenDecodeArguments> tokenDecoder,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _nonceRepository = nonceRepository;
        _tokenDecoder = tokenDecoder;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    protected static bool HasValidState(string? state) => !string.IsNullOrEmpty(state);

    protected static bool HasValidEmptyRedirectUri(string? redirectUri, CachedClient cachedClient)
        => !string.IsNullOrEmpty(redirectUri) || cachedClient.RedirectUris.Count == 1;

    protected static bool HasValidRedirectUri(string? redirectUri, CachedClient cachedClient)
        => string.IsNullOrEmpty(redirectUri) || cachedClient.RedirectUris.Any(x => x == redirectUri);

    protected static bool HasValidResponseMode(string? responseMode)
        => string.IsNullOrEmpty(responseMode) || ResponseModeConstants.ResponseModes.Contains(responseMode);

    protected static bool HasValidResponseType(string? responseType)
        => !string.IsNullOrEmpty(responseType) && ResponseTypeConstants.ResponseTypes.Contains(responseType);

    protected static bool HasValidGrantType(CachedClient cachedClient)
        => cachedClient.GrantTypes.Any(x => x == GrantTypeConstants.AuthorizationCode);

    protected static bool HasValidDisplay(string? display)
        => string.IsNullOrEmpty(display) || DisplayConstants.DisplayValues.Contains(display);

    protected static bool HasValidNonce(string? nonce)
        => !string.IsNullOrEmpty(nonce);

    protected static bool HasValidCodeChallengeMethod(string? codeChallengeMethod)
        => ProofKeyForCodeExchangeHelper.IsCodeChallengeMethodValid(codeChallengeMethod);

    protected static bool HasValidCodeChallenge(string? codeChallenge)
        => ProofKeyForCodeExchangeHelper.IsCodeChallengeValid(codeChallenge);

    protected static bool HasValidScope(IReadOnlyCollection<string> scope)
        => scope.Contains(ScopeConstants.OpenId);

    protected static bool HasAuthorizedScope(IReadOnlyCollection<string> scope, CachedClient cachedClient)
        => !scope.ExceptAny(cachedClient.Scopes);

    protected static bool HasValidMaxAge(string? maxAge)
        => MaxAgeHelper.IsMaxAgeValid(maxAge);

    protected static bool HasValidPrompt(string? prompt)
        => string.IsNullOrEmpty(prompt) || PromptConstants.Prompts.Contains(prompt);

    protected bool HasValidAcrValues(IReadOnlyCollection<string> acrValues)
        => acrValues.Count == 0 || !acrValues.ExceptAny(_discoveryDocumentOptions.Value.AcrValuesSupported);

    protected async Task<bool> HasUniqueNonce(string nonce, CancellationToken cancellationToken)
        => !await _nonceRepository.IsNonceReplay(nonce, cancellationToken);

    protected async Task<bool> HasValidIdTokenHint(string? idTokenHint, string clientId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(idTokenHint))
        {
            return true;
        }

        var validatedToken = await _tokenDecoder.Validate(
            idTokenHint,
            new ServerIssuedTokenDecodeArguments
            {
                ValidateLifetime = true,
                TokenTypes = [TokenTypeHeaderConstants.IdToken],
                Audiences = [clientId]
            }, cancellationToken);

        return validatedToken is not null;
    }
}
