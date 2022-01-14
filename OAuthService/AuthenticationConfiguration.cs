namespace OAuthService;

public record AuthenticationConfiguration
{
    public string Audience { get; init; }

    public string Issuer { get; init; }

    public string TokenSecret { get; init; }
    
    
    public string AuthorizationCodeSecret { get; init; }

    //Seconds
    public int AccessTokenExpiration { get; init; }
    
    //Seconds
    public int RefreshTokenExpiration { get; init; }

    //Seconds
    public int IdTokenExpiration { get; init; }

    //Seconds
    public int AuthorizationCodeExpiration { get; init; }
}