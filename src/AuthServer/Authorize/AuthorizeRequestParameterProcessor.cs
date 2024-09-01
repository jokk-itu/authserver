using System.Net.Http.Headers;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Discovery;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorize;
internal class AuthorizeRequestParameterProcessor : IAuthorizeRequestParameterProcessor
{
    private readonly ITokenDecoder<ClientIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthorizeRequestParameterProcessor> _logger;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    private AuthorizeRequest? _cachedAuthorizeRequest;

    public AuthorizeRequestParameterProcessor(
        ITokenDecoder<ClientIssuedTokenDecodeArguments> tokenDecoder,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthorizeRequestParameterProcessor> logger,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _tokenDecoder = tokenDecoder;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public AuthorizeRequest GetCachedRequest()
    {
        return _cachedAuthorizeRequest ?? throw new InvalidOperationException("authorize request has not been cached");
    }

    public async Task<AuthorizeRequest?> GetRequestByObject(string requestObject, string clientId, CancellationToken cancellationToken)
    {
        try
        {
            var jsonWebToken = await _tokenDecoder.Validate(
                requestObject,
                new ClientIssuedTokenDecodeArguments
                {
                    ValidateLifetime = true,
                    Algorithms = [.. _discoveryDocumentOptions.Value.RequestObjectSigningAlgValuesSupported],
                    Audience = ClientTokenAudience.AuthorizeEndpoint,
                    ClientId = clientId,
                    TokenTypes = [TokenTypeHeaderConstants.RequestObjectToken]
                },
                cancellationToken);

            jsonWebToken.TryGetClaim(Parameter.ClientId, out var clientIdClaim);
            jsonWebToken.TryGetClaim(Parameter.CodeChallenge, out var codeChallengeClaim);
            jsonWebToken.TryGetClaim(Parameter.CodeChallengeMethod, out var codeChallengeMethodClaim);
            jsonWebToken.TryGetClaim(Parameter.Display, out var displayClaim);
            jsonWebToken.TryGetClaim(Parameter.IdTokenHint, out var idTokenHintClaim);
            jsonWebToken.TryGetClaim(Parameter.LoginHint, out var loginHintClaim);
            jsonWebToken.TryGetClaim(Parameter.MaxAge, out var maxAgeClaim);
            jsonWebToken.TryGetClaim(Parameter.Nonce, out var nonceClaim);
            jsonWebToken.TryGetClaim(Parameter.RedirectUri, out var redirectUriClaim);
            jsonWebToken.TryGetClaim(Parameter.Prompt, out var promptClaim);
            jsonWebToken.TryGetClaim(Parameter.ResponseMode, out var responseModeClaim);
            jsonWebToken.TryGetClaim(Parameter.ResponseType, out var responseTypeClaim);
            jsonWebToken.TryGetClaim(Parameter.State, out var stateClaim);
            jsonWebToken.TryGetClaim(Parameter.Scope, out var scopeClaim);
            jsonWebToken.TryGetClaim(Parameter.AcrValues, out var acrValuesClaim);

            _cachedAuthorizeRequest = new AuthorizeRequest
            {
                ClientId = clientIdClaim?.Value ?? string.Empty,
                CodeChallenge = codeChallengeClaim?.Value ?? string.Empty,
                CodeChallengeMethod = codeChallengeMethodClaim?.Value ?? string.Empty,
                Display = displayClaim?.Value ?? string.Empty,
                IdTokenHint = idTokenHintClaim?.Value ?? string.Empty,
                LoginHint = loginHintClaim?.Value ?? string.Empty,
                MaxAge = maxAgeClaim?.Value ?? string.Empty,
                Nonce = nonceClaim?.Value ?? string.Empty,
                RedirectUri = redirectUriClaim?.Value ?? string.Empty,
                Prompt = promptClaim?.Value ?? string.Empty,
                ResponseMode = responseModeClaim?.Value ?? string.Empty,
                ResponseType = responseTypeClaim?.Value ?? string.Empty,
                State = stateClaim?.Value ?? string.Empty,
                Scope = scopeClaim?.Value.Split(' ') ?? [],
                AcrValues = acrValuesClaim?.Value.Split(' ') ?? [],
                RequestUri = string.Empty,
                RequestObject = string.Empty
            };

            return _cachedAuthorizeRequest;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Token validation failed");
            return null;
        }
    }

    public async Task<AuthorizeRequest?> GetRequestByReference(Uri requestUri, string clientId, CancellationToken cancellationToken)
    {
        // TODO implement a Timeout to reduce Denial-Of-Service attacks
        // TODO implement retry delegate handler (5XX and 429)
        var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypeConstants.OAuthRequestJwt));

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var requestObject = await response.Content.ReadAsStringAsync(cancellationToken);
            return await GetRequestByObject(requestObject, clientId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred fetching request_object");
            return null;
        }
    }
}
