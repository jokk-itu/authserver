using System.Text.Json.Serialization;
using AuthServer.Core;

namespace AuthServer.Endpoints.Responses;
internal class PostIntrospectionResponse
{
    [JsonPropertyName(Parameter.Active)]
    public required bool Active { get; init; }

    [JsonPropertyName(Parameter.Scope)]
    public string? Scope { get; init; }

    [JsonPropertyName(Parameter.ClientId)]
    public string? ClientId { get; init; }

    [JsonPropertyName(Parameter.Username)]
    public string? Username { get; init; }

    [JsonPropertyName(Parameter.TokenType)]
    public string? TokenType { get; init; }

    [JsonPropertyName(Parameter.Expires)]
    public long? ExpiresAt { get; init; }

    [JsonPropertyName(Parameter.IssuedAt)]
    public long? IssuedAt { get; init; }

    [JsonPropertyName(Parameter.NotBefore)]
    public long? NotBefore { get; init; }

    [JsonPropertyName(Parameter.Subject)]
    public string? Subject { get; init; }

    [JsonPropertyName(Parameter.Audience)]
    public IEnumerable<string>? Audience { get; init; }

    [JsonPropertyName(Parameter.Issuer)]
    public string? Issuer { get; init; }

    [JsonPropertyName(Parameter.JwtId)]
    public string? JwtId { get; init; }
}