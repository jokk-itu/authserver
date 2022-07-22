using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.RegisterUser;

public record PostRegisterUserRequest
{
  [JsonPropertyName("username")]
  public string Username { get; init; }

  [JsonPropertyName("password")]
  [PasswordPropertyText]
  public string Password { get; init; }

  [JsonPropertyName("email")]
  [EmailAddress]
  public string Email { get; init; }

  [JsonPropertyName("phonenumber")]
  [Phone]
  public string PhoneNumber { get; init; }
}