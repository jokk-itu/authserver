namespace Domain.Constants;
public static class TokenEndpointAuthMethodConstants
{
  public const string ClientSecretPost = "client_secret_post";

  public static string[] TokenEndpointAuthMethods = new[] { ClientSecretPost };
}
