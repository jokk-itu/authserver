using System.Text.Json.Serialization;

namespace Contracts.GetJwksDocument;

#nullable disable
public class JwkDto
{
  [JsonPropertyName("kty")]
  public static string KeyType => "RSA";

  [JsonPropertyName("use")]
  public static string Use => "sig";

  [JsonPropertyName("kid")]
  public long KeyId { get; init; }

  [JsonPropertyName("alg")]
  public static string Alg => "RS256";

  [JsonPropertyName("n")]
  public string Modulus { get; init; }

  [JsonPropertyName("e")]
  public string Exponent { get; init; }
}
