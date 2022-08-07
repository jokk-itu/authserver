using System.Text.Json.Serialization;

namespace Contracts.GetJwksDocument;
public class JwkDto
{
  [JsonPropertyName("kty")]
  public string KeyType => "RSA";

  [JsonPropertyName("use")]
  public string Use => "sig";

  [JsonPropertyName("kid")]
  public long KeyId { get; init; }

  [JsonPropertyName("alg")]
  public string Alg => "RS256";

  [JsonPropertyName("n")]
  public string Modulus { get; init; }

  [JsonPropertyName("e")]
  public string Exponent { get; init; }
}
