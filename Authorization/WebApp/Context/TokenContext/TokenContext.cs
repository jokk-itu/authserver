namespace WebApp.Context.TokenContext;

#nullable disable
public class TokenContext
{
    public string GrantType { get; set; }
    public string Code { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public string CodeVerifier { get; set; }
    public string RefreshToken { get; set; }
    public ICollection<string> Resource { get; set; }
}