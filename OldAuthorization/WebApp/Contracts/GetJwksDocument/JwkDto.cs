using System.Text.Json.Serialization;

namespace WebApp.Contracts.GetJwksDocument;

#nullable disable
public record JwkDto
{
  [JsonPropertyName("kty")]
  public string KeyType { get; init; }

  [JsonPropertyName("use")]
  public string Use { get; init; }

  [JsonPropertyName("kid")]
  public string KeyId { get; init; }

  [JsonPropertyName("alg")]
  public string Alg { get; init; }

  [JsonPropertyName("n")]
  public string Modulus { get; init; }

  [JsonPropertyName("e")]
  public string Exponent { get; init; }
}