namespace AuthorizationServer.Constants;

public static class AuthorizeCodeErrorDescription
{
    public const string UserInformation = "userlogin is missing";
    public const string ResponseType = "Supported values are: id_token, id_token code, code. Disclaimer: The order does not matter.";
    public const string ClientId = "client_id is missing";
    public const string RedirectUri = "redirect_uri is missing";
    public const string Scope = "scope is missing";
    public const string State = "state is missing";
    public const string CodeChallenge = "codechallenge is missing";
    public const string CodeChallengeMethod = "Supported values are: S256 and plain";
    public const string Nonce = "nonce is missing";
    public const string Display = "Supported values are: page, popup, touch and wap";
}