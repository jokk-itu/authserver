namespace AuthServer.Constants;

/// <summary>
/// https://www.iana.org/assignments/media-types/media-types.xhtml
/// </summary>
public class TokenTypeHeaderConstants
{
    public const string PrivateKeyToken = "pk+jwt";
    public const string AccessToken = "at+jwt";
    public const string RefreshToken = "refresh+jwt";
    public const string IdToken = "id+jwt";
    public const string DPoPToken = "dpop+jwt";
    public const string LogoutToken = "logout+jwt";
    public const string RequestObjectToken = "oauth-authz-req+jwt";
    public const string UserinfoToken = "userinfo+jwt";
}