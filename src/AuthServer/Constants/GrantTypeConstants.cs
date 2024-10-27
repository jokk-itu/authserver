namespace AuthServer.Constants;
public static class GrantTypeConstants
{
    public const string AuthorizationCode = "authorization_code";
    public const string RefreshToken = "refresh_token";
    public const string ClientCredentials = "client_credentials";

    public static readonly string[] GrantTypes = [AuthorizationCode, RefreshToken, ClientCredentials];
}