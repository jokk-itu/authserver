using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Contracts.ResetPassword;

public record PostResetPasswordRequest
{
  [Required(AllowEmptyStrings = false)]
  public string Username { get; init; }

  [Required(AllowEmptyStrings = false)]
  [PasswordPropertyText]
  public string Password { get; init; }
}