namespace WebApp.Context.AuthorizeContext;

#nullable disable
public class AuthorizeContext
{
    public string Scope { get; set; }
    public string IdTokenHint { get; set; }
    public string Prompt { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeChallengeMethod { get; set; }
    public string ResponseType { get; set; }
    public string Nonce { get; set; }
    public string MaxAge { get; set; }
    public string State { get; set; }
    public string ResponseMode { get; set; }
}