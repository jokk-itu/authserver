using System.Text.Json.Serialization;

namespace Contracts.GetJwksDocument;

#nullable disable
public record JwkDto
{
  [JsonPropertyName("kty")]
  public string KeyType { get; } = "RSA";

  [JsonPropertyName("use")]
  public string Use { get; } = "sig";

  [JsonPropertyName("kid")]
  public long KeyId { get; init; }

  [JsonPropertyName("alg")]
  public string Alg { get; } = "RS256";

  [JsonPropertyName("n")]
  public string Modulus { get; init; }

  [JsonPropertyName("e")]
  public string Exponent { get; init; }
}