using System.Text.Json.Serialization;

namespace Contracts.GetTokenIntrospeciton;
#nullable enable
public record GetTokenIntrospectionResponse
{
  [JsonPropertyName("active")]
  public bool Active { get; init; }

  [JsonPropertyName("scope")]
  public string Scope { get; init; } = null!;

  [JsonPropertyName("sub")]
  public string? SubjectId { get; init; }

  [JsonPropertyName("aud")]
  public string Audience { get; init; } = null!;

  [JsonPropertyName("iss")]
  public string Issuer { get; init; } = null!;

  [JsonPropertyName("exp")]
  public long Expires { get; init; }

  [JsonPropertyName("iat")]
  public long IssuedAt { get; init; }
}