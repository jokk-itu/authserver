using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Contracts.RegisterUser;

public record PostRegisterUserRequest
{
  [JsonPropertyName("username")]
  public string Username { get; init; } = null!;

  [JsonPropertyName("password")]
  [PasswordPropertyText]
  public string Password { get; init; } = null!;

  [JsonPropertyName("email")]
  [EmailAddress]
  public string Email { get; init; } = null!;

  [JsonPropertyName("phonenumber")]
  [Phone]
  public string PhoneNumber { get; init; } = null!;

  [JsonPropertyName("address")]
  public string Address { get; init; } = null!;

  [JsonPropertyName("family_name")]
  public string FamilyName { get; init; } = null!;

  [JsonPropertyName("given_name")]
  public string GivenName { get; init; } = null!;

  [JsonPropertyName("middle_name")]
  public string? MiddleName { get; init; }

  [JsonPropertyName("nickname")]
  public string NickName { get; init; } = null!;

  [JsonPropertyName("gender")]
  public string Gender { get; init; } = null!;

  [JsonPropertyName("birthdate")]
  public DateTime BirthDate { get; init; }

  [JsonPropertyName("locale")]
  public string Locale { get; init; } = null!;
}