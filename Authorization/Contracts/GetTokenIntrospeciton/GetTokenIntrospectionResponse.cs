using System.Text.Json.Serialization;

namespace Contracts.GetTokenIntrospeciton;
#nullable enable
public class GetTokenIntrospectionResponse
{
  [JsonPropertyName("active")]
  public bool Active { get; set; }

  [JsonPropertyName("scope")]
  public string Scope { get; set; } = null!;

  [JsonPropertyName("sub")]
  public string? SubjectId { get; set; }

  [JsonPropertyName("aud")]
  public string Audience { get; set; } = null!;

  [JsonPropertyName("iss")]
  public string Issuer { get; set; } = null!;

  [JsonPropertyName("exp")]
  public long Expires { get; set; }

  [JsonPropertyName("iat")]
  public long IssuedAt { get; set; }
}