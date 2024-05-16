using System.Text.Json.Serialization;

namespace WebApp.Contracts;
public class ErrorResponse
{
  public ErrorResponse()
  {
    
  }

  public ErrorResponse(string error, string? errorDescription)
  {
    Error = error;
    ErrorDescription = errorDescription;
  }

  [JsonPropertyName("error")]
  public string Error { get; set; } = null!;

  [JsonPropertyName("error_description")]
  public string? ErrorDescription { get; set; }
}
