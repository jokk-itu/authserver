using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Mvc;

namespace OAuthService.Requests;

public record AuthorizeRequest
{
    [FromBody] 
    public UserLoginRequest UserInformation { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "response_type")] 
    public string ResponseType { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "client_id")] 
    public string ClientId { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "redirect_uri")] 
    public string RedirectUri { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "scope")] 
    public string Scope { get; init; }
    
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "state")] 
    public string State { get; init; }
    
    [RegularExpression("^[a-zA-Z0-9-_~.]{43,128}$")]
    [Required(AllowEmptyStrings = false)] 
    [FromQuery(Name = "code_challenge")] 
    public string CodeChallenge { get; init; }

    [RegularExpression("plain|S256")]
    [FromQuery(Name = "code_challenge_method")]
    public string CodeChallengeMethod => "plain";
    
    [FromQuery(Name = "nonce")]
    public string Nonce { get; init; }

    [RegularExpression("page|popup|touch|wap")]
    [FromQuery(Name = "display")] 
    public string Display => "page";
}