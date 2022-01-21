namespace OAuthService.Entities;

public class IdentityClientRedirectUri<TKey>
{
    public string Uri { get; init; }

    public TKey ClientId { get; init; }
}