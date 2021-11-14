using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OAuthService.Requests;

public class RegisterUserRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false)]
    [PasswordPropertyText]
    public string Password { get; set; }

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }
}