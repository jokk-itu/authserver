using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OAuthService.Requests;

public class UserLoginRequest
{
  [Required(AllowEmptyStrings = false)]
  public string Username { get; init; }

  [Required(AllowEmptyStrings = false)]
  [PasswordPropertyText]
  public string Password { get; init; }
}