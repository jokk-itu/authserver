using Microsoft.EntityFrameworkCore;

namespace OAuthService.Repositories;

public class ClientManager
{
    private readonly IdentityContext _context;

    public ClientManager(IdentityContext context)
    {
        _context = context;
    }

    public async Task<bool> IsValidClient(string clientId, string? clientSecret = null)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("ClientId is empty or null", nameof(clientId));

        if (clientSecret is not null && clientSecret.Equals(string.Empty))
            throw new ArgumentException("ClientSecret must not be empty", nameof(clientSecret));

        var client = await _context.Clients.FindAsync(clientId);

        if (clientSecret is not null)
            return client is not null && clientSecret.Sha256().Equals(client.SecretHash);
        
        return client is not null;
    }

    public async Task<bool> IsValidScopes(string clientId, ICollection<string> scopes)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("ClientId is null or empty", nameof(clientId));

        if (scopes is null)
            throw new ArgumentNullException(nameof(scopes));

        var clientScopes = await _context.ClientScopes
            .Where(cs => cs.ClientId.Equals(clientId))
            .ToListAsync();
        
        var isValid = true;
        using var enumerator = scopes.GetEnumerator();
        while (isValid && enumerator.MoveNext())
        {
            if (!clientScopes.Exists(cs => cs.Name.Equals(enumerator.Current)))
                isValid = false;
        }

        return isValid;
    }
    
    public async Task<bool> IsValidGrants(string clientId, ICollection<string> grants)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("ClientId is null or empty", nameof(clientId));

        if (grants is null)
            throw new ArgumentNullException(nameof(grants));

        var clientGrants = await _context.ClientGrants
            .Where(cs => cs.ClientId.Equals(clientId))
            .ToListAsync();
        
        var isValid = true;
        using var enumerator = grants.GetEnumerator();
        while (isValid && enumerator.MoveNext())
        {
            if (!clientGrants.Exists(cs => cs.Name.Equals(enumerator.Current)))
                isValid = false;
        }

        return isValid;
    }
    
    public async Task<bool> IsValidRedirectUris(string clientId, ICollection<string> redirectUris)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("ClientId is null or empty", nameof(clientId));

        if (redirectUris is null)
            throw new ArgumentNullException(nameof(redirectUris));

        var clientRedirectUris = await _context.ClientRedirectUris
            .Where(cs => cs.ClientId.Equals(clientId))
            .ToListAsync();
        
        var isValid = true;
        using var enumerator = redirectUris.GetEnumerator();
        while (isValid && enumerator.MoveNext())
        {
            if (!clientRedirectUris.Exists(cs => cs.Uri.Equals(enumerator.Current)))
                isValid = false;
        }

        return isValid;
    }
}