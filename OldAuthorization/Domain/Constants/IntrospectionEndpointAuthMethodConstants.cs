namespace Domain.Constants;
public static class IntrospectionEndpointAuthMethodConstants
{
  public const string ClientSecretPost = "client_secret_post";
  public const string ClientSecretBasic = "client_secret_basic";
  public const string None = "none";

  public static readonly string[] AuthMethods = { ClientSecretPost, ClientSecretBasic, None };
}