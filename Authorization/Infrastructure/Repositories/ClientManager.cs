using AuthorizationServer.Entities;
using AuthorizationServer.Extensions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AuthorizationServer.Repositories;

public class ClientManager
{
  private readonly IdentityContext _context;

  public ClientManager(IdentityContext context)
  {
    _context = context;
  }

  public async Task<IdentityClient?> FindClientByIdAsync(
      [Required] string clientId)
  {
    return await _context.Clients.FindAsync(clientId);
  }

  public async Task<bool> IsValidClientAsync(
      [Required] string clientId,
      string? clientSecret = null)
  {
    if (clientSecret is not null && string.IsNullOrEmpty(clientSecret))
      throw new ArgumentException("ClientSecret must not be empty", nameof(clientSecret));

    var client = await _context.Clients.FindAsync(clientId);

    if (clientSecret is not null)
      return client is not null && clientSecret.Sha256().Equals(client.SecretHash);

    return client is not null;
  }

  public async Task<bool> IsValidScopesAsync(
      [Required] string clientId,
      ICollection<string> scopes)
  {
    if (scopes is null)
      throw new ArgumentNullException(nameof(scopes));

    var clientScopes = await _context.ClientScopes
        .Where(cs => cs.ClientId.Equals(clientId))
        .ToListAsync();

    var isValid = true;
    using var enumerator = scopes.GetEnumerator();
    while (isValid && enumerator.MoveNext())
    {
      if (!clientScopes.Exists(cs => cs.ScopeId.Equals(enumerator.Current)))
        isValid = false;
    }

    return isValid;
  }

  public async Task<bool> IsValidGrantsAsync(
      [Required] string clientId,
      ICollection<string> grants)
  {
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

  public async Task<bool> IsValidRedirectUrisAsync(
      [Required] string clientId,
      ICollection<string> redirectUris)
  {
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

  public async Task SetTokenAsync(
      [Required] string clientId,
      [Required] string tokenName,
      [Required] string tokenValue)
  {
    await _context.ClientTokens.AddAsync(new IdentityClientToken<string>
    {
      ClientId = clientId,
      Name = tokenName,
      Value = tokenValue
    });
    await _context.SaveChangesAsync();
  }
}