namespace Domain.Constants;
public static class TokenEndpointAuthMethodConstants
{
  public const string ClientSecretPost = "client_secret_post";
  public const string ClientSecretBasic = "client_secret_basic";
  public const string None = "none";

  public static readonly string[] TokenEndpointAuthMethods = { ClientSecretPost, ClientSecretBasic, None };
}