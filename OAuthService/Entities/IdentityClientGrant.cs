namespace OAuthService.Entities;

public class IdentityClientGrant<TKey>
{
    public string Name { get; set; }

    public TKey ClientId { get; set; }
}