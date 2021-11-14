namespace OAuthService.Requests;

public record AuthorizeRequest
{
    public string Username { get; init; }
    
    public string Password { get; init; }
}