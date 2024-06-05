using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;
internal class ClientRepository : IClientRepository
{
    private readonly AuthorizationDbContext _identityContext;

    public ClientRepository(AuthorizationDbContext identityContext)
    {
        _identityContext = identityContext;
    }

    /// <inheritdoc/>
    public async Task<bool> DoesResourcesExist(IReadOnlyCollection<string> resources, IReadOnlyCollection<string> scopes, CancellationToken cancellationToken)
    {
        var resourcesExisting = await _identityContext
            .Set<Client>()
            .Where(r => r.ClientUri != null && resources.Contains(r.ClientUri))
            .Where(r => r.Scopes.AsQueryable().Any(s => scopes.Contains(s.Name)))
            .CountAsync(cancellationToken: cancellationToken);

        return resourcesExisting == resources.Count;
    }
}