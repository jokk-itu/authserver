namespace OAuthService.Entities;

public class IdentityClientScope<TKey>
{
    public string ScopeId { get; init; }

    public TKey ClientId { get; init; }
}