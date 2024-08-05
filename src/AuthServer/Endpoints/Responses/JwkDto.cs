using System.Text.Json.Serialization;

namespace AuthServer.Endpoints.Responses;
internal class JwkDto
{
    [JsonPropertyName("kty")]
    public required string KeyType { get; init; }

    [JsonPropertyName("use")]
    public required string Use { get; init; }

    [JsonPropertyName("kid")]
    public required string KeyId { get; init; }

    [JsonPropertyName("alg")]
    public required string Alg { get; init; }

    [JsonPropertyName("key_ops")]
    public required IEnumerable<string> KeysOps { get; init; }

    [JsonPropertyName("n")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Modulus { get; set; }

    [JsonPropertyName("e")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Exponent { get; set; }

    [JsonPropertyName("x5t")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? X509Thumbprint { get; set; }

    [JsonPropertyName("crv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Curve { get; set; }

    [JsonPropertyName("x")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? X { get; set; }

    [JsonPropertyName("y")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Y { get; set; }
}
