using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace OAuthService.Requests;

public record AuthorizeRequest
{
    [FromBody] 
    public UserLoginRequest UserInformation { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "response_type")] 
    public string responseType { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "client_id")] 
    public string clientId { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "redirect_uri")] 
    public string redirectUri { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "scope")] 
    public string scope { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "state")] 
    public string state { get; init; }
    
    [RegularExpression("^[a-zA-Z0-9-_~.]{43,128}$")]
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "code_challenge")] 
    public string codeChallenge { get; init; }

    [RegularExpression("plain|S256")]
    [FromQuery(Name = "code_challenge_method")]
    public string codeChallengeMethod => "plain";
}