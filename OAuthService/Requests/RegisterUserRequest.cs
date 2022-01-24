using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OAuthService.Requests;

public record RegisterUserRequest
{
  [Required(AllowEmptyStrings = false)]
  public string Username { get; init; }

  [Required(AllowEmptyStrings = false)]
  [PasswordPropertyText]
  public string Password { get; init; }

  [Required(AllowEmptyStrings = false)]
  [EmailAddress]
  public string Email { get; init; }

  [Required(AllowEmptyStrings = false)]
  [Phone]
  public string PhoneNumber { get; init; }
}