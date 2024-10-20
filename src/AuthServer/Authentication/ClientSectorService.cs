using System.Text.Json;
using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using Microsoft.Extensions.Logging;

namespace AuthServer.Authentication;
internal class ClientSectorService : IClientSectorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientSectorService> _logger;

    public ClientSectorService(
        IHttpClientFactory httpClientFactory,
        ILogger<ClientSectorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<bool> ContainsSectorDocument(Uri sectorIdentifierUri, IReadOnlyCollection<string> uris, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(HttpClientNameConstants.Client);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, sectorIdentifierUri);
        httpRequestMessage.Headers.Add("Accept", MimeTypeConstants.Json);

        try
        {
            _logger.LogDebug("Requesting sector_identifier_uri {Uri}", sectorIdentifierUri);

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();

            var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            var sectorUris = JsonSerializer.Deserialize<IEnumerable<string>>(content);
            if (sectorUris is null)
            {
                _logger.LogInformation("Unrecognizable content from sector_identifier_uri {Uri}", sectorIdentifierUri);
                return false;
            }

            return uris.All(sectorUris.Contains);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error occurred requesting sector_identifier_uri {Uri}", sectorIdentifierUri);
            return false;
        }
    }
}
