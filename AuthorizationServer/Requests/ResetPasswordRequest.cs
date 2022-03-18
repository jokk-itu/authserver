using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Requests;

public record ResetPasswordRequest
{
  [Required(AllowEmptyStrings = false)]
  public string Username { get; init; }

  [Required(AllowEmptyStrings = false)]
  [PasswordPropertyText]
  public string Password { get; init; }
}