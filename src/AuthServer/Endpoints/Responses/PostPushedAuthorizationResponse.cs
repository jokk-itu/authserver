using System.Text.Json.Serialization;
using AuthServer.Core;

namespace AuthServer.Endpoints.Responses;
internal class PostPushedAuthorizationResponse
{
    [JsonPropertyName(Parameter.RequestUri)]
    public required string RequestUri { get; init; }

    [JsonPropertyName(Parameter.ExpiresIn)]
    public required int ExpiresIn { get; init; }
}
