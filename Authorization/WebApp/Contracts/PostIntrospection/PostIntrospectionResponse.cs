using System.Text.Json.Serialization;
using WebApp.Constants;

namespace WebApp.Contracts.PostIntrospection;

public class PostIntrospectionResponse
{
  [JsonPropertyName(ParameterNames.Active)]
  public bool Active { get; init; }

  [JsonPropertyName(ParameterNames.Scope)]
  public string? Scope { get; init; }

  [JsonPropertyName(ParameterNames.ClientId)]
  public string? ClientId { get; init; }

  [JsonPropertyName(ParameterNames.Username)]
  public string? Username { get; init; }

  [JsonPropertyName(ParameterNames.TokenType)]
  public string? TokenType { get; init; }

  [JsonPropertyName(ParameterNames.Expires)]
  public long? ExpiresAt { get; init; }

  [JsonPropertyName(ParameterNames.IssuedAt)]
  public long? IssuedAt { get; init; }

  [JsonPropertyName(ParameterNames.NotBefore)]
  public long? NotBefore { get; init; }

  [JsonPropertyName(ParameterNames.Subject)]
  public string? Subject { get; init; }

  [JsonPropertyName(ParameterNames.Audience)]
  public IEnumerable<string> Audience { get; init; } = new List<string>();

  [JsonPropertyName(ParameterNames.Issuer)]
  public string? Issuer { get; init; }

  [JsonPropertyName(ParameterNames.JwtId)]
  public string? JwtId { get; init; }
}