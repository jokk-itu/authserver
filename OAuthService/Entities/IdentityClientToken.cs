using Microsoft.AspNetCore.Identity;

namespace OAuthService.Entities;

public class IdentityClientToken<TKey>
{
    public TKey ClientId { get; set; }

    public string Name { get; set; }

    [ProtectedPersonalData]
    public string Value { get; set; }
}