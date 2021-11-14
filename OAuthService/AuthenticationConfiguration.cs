namespace OAuthService;

public record AuthenticationConfiguration
{
    public string Audience { get; init; }

    public string Issuer { get; init; }

    public string TokenSecret { get; init; }
    
    public string CodeSecret { get; init; }

    //Seconds
    public int AccessTokenExpiration { get; init; }
    
    //Seconds
    public int RefreshTokenExpiration { get; init; }

    //Seconds
    public int IdTokenExpiration { get; set; }

    //Seconds
    public int AuthorizationCodeTokenExpiration { get; set; }
}