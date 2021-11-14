namespace OAuthService.Entities;

public class IdentityClientRedirectUri<TKey>
{
    public string Uri { get; set; }

    public TKey ClientId { get; set; }
}