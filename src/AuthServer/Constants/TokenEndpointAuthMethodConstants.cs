namespace AuthServer.Constants;
public static class TokenEndpointAuthMethodConstants
{
    public const string ClientSecretPost = "client_secret_post";
    public const string ClientSecretBasic = "client_secret_basic";
    public const string None = "none";
    public const string PrivateKeyJwt = "private_key_jwt";

    public static readonly string[] AuthMethods = [ClientSecretPost, ClientSecretBasic, None, PrivateKeyJwt];
    public static readonly string[] SecureAuthMethods = [ClientSecretBasic, ClientSecretPost, PrivateKeyJwt];
}