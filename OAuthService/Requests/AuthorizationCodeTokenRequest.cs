using System.ComponentModel.DataAnnotations;

namespace OAuthService.Requests;

public record AuthorizationCodeTokenRequest
{
    [Required(AllowEmptyStrings = false)]
    [RegularExpression("authorization_code|refresh_token")]
    public string grant_type { get; init; }
    
    public string? code { get; init; }

    [Required(AllowEmptyStrings = false)]
    public string redirect_uri { get; init; }

    public string? refresh_token { get; init; }

    public string? scope { get; init; }
    
    public string? code_verifier { get; init; }
}