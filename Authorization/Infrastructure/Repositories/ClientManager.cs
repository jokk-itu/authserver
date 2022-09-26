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

    if (client.Secret == clientSecret) 
      return true;

    _logger.LogDebug("ClientSecret {ClientSecret} is wrong", clientSecret);
    return false;

  }
}