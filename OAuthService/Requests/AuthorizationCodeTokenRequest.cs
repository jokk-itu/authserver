using System.ComponentModel.DataAnnotations;

namespace OAuthService.Requests;

public record AuthorizationCodeTokenRequest
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression("authorization_code")]
    public string grant_type { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string code { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string redirect_uri { get; init; }
}