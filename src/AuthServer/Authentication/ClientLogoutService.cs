using AuthServer.Authentication.Abstractions;
using AuthServer.Cache.Abstractions;
using AuthServer.Core;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.Extensions.Logging;

namespace AuthServer.Authentication;
internal class ClientLogoutService : IClientLogoutService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenBuilder<LogoutTokenArguments> _tokenBuilder;
    private readonly ICachedClientStore _cachedClientStore;
    private readonly ILogger<ClientLogoutService> _logger;

    public ClientLogoutService(
        IHttpClientFactory httpClientFactory,
        ITokenBuilder<LogoutTokenArguments> tokenBuilder,
        ICachedClientStore cachedClientStore,
        ILogger<ClientLogoutService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _tokenBuilder = tokenBuilder;
        _cachedClientStore = cachedClientStore;
        _logger = logger;
    }

    public async Task Logout(string clientId, string? sessionId, string? subjectIdentifier, CancellationToken cancellationToken)
    {
        var client = await _cachedClientStore.Get(clientId, cancellationToken);

        var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);
        var logoutToken = await _tokenBuilder.BuildToken(new LogoutTokenArguments
        {
            ClientId = clientId,
            SessionId = sessionId,
            SubjectIdentifier = subjectIdentifier
        }, cancellationToken);

        var body = new Dictionary<string, string>
        {
            { Parameter.LogoutToken, logoutToken }
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, client.BackchannelLogoutUri)
        {
            Content = new FormUrlEncodedContent(body)
        };

        // TODO Implement retry for 5XX and 429
        // TODO Implement Timeout to remove denial-of-service attacks

        try
        {
            var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning(e, "Error occurred requesting logout for client {ClientId}", clientId);
        }
    }
}
