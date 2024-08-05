using System.Text.Json.Serialization;

namespace AuthServer.Endpoints.Responses;
internal class GetJwksResponse
{
    [JsonPropertyName("keys")]
    public required IEnumerable<JwkDto> Keys { get; init; }
}
