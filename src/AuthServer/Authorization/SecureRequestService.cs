using System.Net.Http.Headers;
using AuthServer.Authorization.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Options;
using AuthServer.Repositories.Abstractions;
using AuthServer.TokenDecoders;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthServer.Authorization;

internal class SecureRequestService : ISecureRequestService
{
    private readonly ITokenDecoder<ClientIssuedTokenDecodeArguments> _tokenDecoder;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SecureRequestService> _logger;
    private readonly IClientRepository _clientRepository;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    private AuthorizeRequestDto? _cachedAuthorizeRequestObjectDto;

    public SecureRequestService(
        ITokenDecoder<ClientIssuedTokenDecodeArguments> tokenDecoder,
        IHttpClientFactory httpClientFactory,
        ILogger<SecureRequestService> logger,
        IClientRepository clientRepository,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _tokenDecoder = tokenDecoder;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _clientRepository = clientRepository;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    /// <inheritdoc/>
    public AuthorizeRequestDto GetCachedRequest()
    {
        return _cachedAuthorizeRequestObjectDto ??
               throw new InvalidOperationException("authorize request has not been cached");
    }

    /// <inheritdoc/>
    public async Task<AuthorizeRequestDto?> GetRequestByObject(string requestObject, string clientId,
        ClientTokenAudience audience, CancellationToken cancellationToken)
    {
        var jsonWebToken = await _tokenDecoder.Validate(
            requestObject,
            new ClientIssuedTokenDecodeArguments
            {
                ValidateLifetime = true,
                Algorithms = [.. _discoveryDocumentOptions.Value.RequestObjectSigningAlgValuesSupported],
                Audience = audience,
                ClientId = clientId,
                TokenType = TokenTypeHeaderConstants.RequestObjectToken
            },
            cancellationToken);

        if (jsonWebToken is null)
        {
            return null;
        }

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

        _cachedAuthorizeRequestObjectDto = new AuthorizeRequestDto
        {
            ClientId = clientIdClaim?.Value,
            CodeChallenge = codeChallengeClaim?.Value,
            CodeChallengeMethod = codeChallengeMethodClaim?.Value,
            Display = displayClaim?.Value,
            IdTokenHint = idTokenHintClaim?.Value,
            LoginHint = loginHintClaim?.Value,
            MaxAge = maxAgeClaim?.Value,
            Nonce = nonceClaim?.Value,
            RedirectUri = redirectUriClaim?.Value,
            Prompt = promptClaim?.Value,
            ResponseMode = responseModeClaim?.Value,
            ResponseType = responseTypeClaim?.Value,
            State = stateClaim?.Value,
            Scope = scopeClaim?.Value.Split(' ') ?? [],
            AcrValues = acrValuesClaim?.Value.Split(' ') ?? [],
        };

        return _cachedAuthorizeRequestObjectDto;
    }

    /// <inheritdoc/>
    public async Task<AuthorizeRequestDto?> GetRequestByReference(Uri requestUri, string clientId,
        ClientTokenAudience audience, CancellationToken cancellationToken)
    {
        // TODO implement a Timeout to reduce Denial-Of-Service attacks, where a RequestUri recursively calls Authorize
        // TODO implement retry delegate handler (5XX and 429)
        var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypeConstants.OAuthRequestJwt));

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var requestObject = await response.Content.ReadAsStringAsync(cancellationToken);
            return await GetRequestByObject(requestObject, clientId, audience, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred fetching request_object");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<AuthorizeRequestDto?> GetRequestByPushedRequest(string requestUri, string clientId,
        CancellationToken cancellationToken)
    {
        var reference = requestUri[RequestUriConstants.RequestUriPrefix.Length..];

        _cachedAuthorizeRequestObjectDto =
            await _clientRepository.GetAuthorizeDto(reference, clientId, cancellationToken);
        return _cachedAuthorizeRequestObjectDto;
    }
}