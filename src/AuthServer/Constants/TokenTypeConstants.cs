namespace AuthServer.Constants;
public static class TokenTypeConstants
{
    public const string RefreshToken = "refresh_token";

    public const string AccessToken = "access_token";

    public static readonly string[] TokenTypes = [RefreshToken, AccessToken];
}