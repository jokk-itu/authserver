namespace OAuthService.Entities;

public class IdentityClientScope<TKey>
{
    public string Name { get; set; }

    public TKey ClientId { get; set; }
}