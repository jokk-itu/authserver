using Domain;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class ClientManager
{
  private readonly IdentityContext _identityContext;
  private readonly ILogger<ClientManager> _logger;

  public ClientManager(IdentityContext identityContext, ILogger<ClientManager> logger)
  {
    _identityContext = identityContext;
    _logger = logger;
  }

  public async Task<Client?> ReadClientAsync(string clientId, CancellationToken cancellationToken = default)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(client => client.Scopes)
      .Include(client => client.Grants)
      .Include(client => client.RedirectUris)
      .SingleOrDefaultAsync(client => client.Id == clientId, cancellationToken: cancellationToken);

    if (client is null)
      _logger.LogDebug("Client {ClientId} not found", clientId);

    return client;
  }

  public bool Login(string clientSecret, Client client)
  {
    if (string.IsNullOrWhiteSpace(clientSecret))
      throw new ArgumentException("Must not be null or only whitespace", nameof(clientSecret));

    if (client is null)
      throw new ArgumentNullException(nameof(client));

    if (client.SecretHash == clientSecret.Sha256())
    {
      _logger.LogDebug("ClientSecret {ClientSecret} is wrong", clientSecret);
      return false;
    }

    return true;
  }

  public bool IsAuthorizedRedirectUris(Client client, IEnumerable<string> redirectUris)
  {
    if (client is null)
      throw new ArgumentNullException(nameof(client));

    if (redirectUris is null || !redirectUris.Any())
      throw new ArgumentException("Must not be null or empty", nameof(redirectUris));

    foreach (var redirectUri in redirectUris)
    {
      if (!client.RedirectUris.Any(r => r.Uri == redirectUri))
      {
        _logger.LogDebug("Client with clientId {ClientId} is not authorized to use redirectUri {RedirectUri}", client.Id, redirectUri);
        return false;
      }
    }

    return true;
  }

  public async Task<bool> IsAuthorizedGrants(Client client, IEnumerable<string> grants)
  {
    if (client is null)
      throw new ArgumentNullException(nameof(client));

    if (grants is null || !grants.Any())
      throw new ArgumentException("Must not be null or empty", nameof(grants));

    foreach (var grant in grants)
    {
      if (!client.Grants.Any(g => g.Name == grant))
      {
        _logger.LogDebug("Client {ClientId} is not authorized to use {Grant}", client.Id, grant);
        return false;
      }
    }

    return true;
  }

  public bool IsAuthorizedScopes(Client client, IEnumerable<string> scopes)
  {
    if (client is null)
      throw new ArgumentNullException(nameof(client));

    if (scopes is null || !scopes.Any())
      throw new ArgumentException("Must not be null or empty", nameof(scopes));

    foreach (var scope in scopes)
    {
      if (!client.Scopes.Any(s => s.Name == scope))
      {
        _logger.LogDebug("Client {ClientId} is not authorized to use {Scope}", client.Id, scope);
        return false;
      }
    }

    return true;
  }
}