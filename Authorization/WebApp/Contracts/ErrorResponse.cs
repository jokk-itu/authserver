using System.Text.Json.Serialization;

namespace Contracts;
public class ErrorResponse
{
  [JsonPropertyName("error")]
  public string Error { get; set; } = null!;

  [JsonPropertyName("error_description")]
  public string? ErrorDescription { get; set; }
}
