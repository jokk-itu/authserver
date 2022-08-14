namespace AuthorizationServer.Constants;

public static class AuthorizeCodeErrorDescription
{
  public const string Username = "username is missing";
  public const string Password = "password is missing";
  public const string ResponseType = "Supported values are: id_token, code id_token, code.";
  public const string ClientId = "client_id is missing";
  public const string ClientSecret = "client_secret is missing";
  public const string RedirectUri = "redirect_uri is missing";
  public const string Scope = "scope is missing";
  public const string State = "state is missing";
  public const string CodeChallenge = "codechallenge is missing";
  public const string CodeChallengeMethod = "Supported values are: S256";
  public const string Nonce = "nonce is missing";
  public const string Display = "Supported values are: page, popup, touch and wap";
  public const string Prompt = "Suported values are: none, login";
}