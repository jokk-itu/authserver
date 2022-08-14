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

  [JsonPropertyName("address")]
  public string Address { get; set; }

  [JsonPropertyName("family_name")]
  public string FamilyName { get; set; }

  [JsonPropertyName("given_name")]
  public string GivenName { get; set; }

  [JsonPropertyName("middle_name")]
  public string? MiddleName { get; set; }

  [JsonPropertyName("nickname")]
  public string NickName { get; set; }

  [JsonPropertyName("gender")]
  public string Gender { get; set; }

  [JsonPropertyName("birthdate")]
  public DateTime BirthDate { get; set; }

  [JsonPropertyName("locale")]
  public string Locale { get; set; }
}