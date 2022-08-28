using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Contracts.ResetPassword;
#nullable disable
public record PostResetPasswordRequest
{
  [JsonPropertyName("username")]
  public string Username { get; init; }

  [JsonPropertyName("password")]
  [PasswordPropertyText]
  public string Password { get; init; }
}