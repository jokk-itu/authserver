using System.Text.Json.Serialization;
using AuthServer.Core;

namespace AuthServer.Endpoints.Responses;

internal class PostTokenResponse
{
    [JsonPropertyName(Parameter.AccessToken)]
    public required string AccessToken { get; init; }

    [JsonPropertyName(Parameter.RefreshToken)]
    public string? RefreshToken { get; init; }

    [JsonPropertyName(Parameter.IdToken)]
    public string? IdToken { get; init; }

    [JsonPropertyName(Parameter.TokenType)]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName(Parameter.ExpiresIn)]
    public required long ExpiresIn { get; init; }

    [JsonPropertyName(Parameter.Scope)]
    public required string Scope { get; init; }
}