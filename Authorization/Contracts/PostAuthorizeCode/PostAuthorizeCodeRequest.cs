using System.Text.Json.Serialization;

namespace Contracts.AuthorizeCode;

public record PostAuthorizeCodeRequest
{
  [JsonPropertyName("username")]
  public string Username { get; init; }

  [JsonPropertyName("password")]
  public string Password { get; init; }
}