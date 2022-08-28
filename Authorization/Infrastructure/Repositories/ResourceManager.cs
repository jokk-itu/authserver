using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ResourceManager
{
  private readonly IdentityContext _identityContext;

  public ResourceManager(IdentityContext context)
  {
    _identityContext = context;
  }

  public async Task<ICollection<Resource>> ReadResourcesAsync(ICollection<string> scopes, CancellationToken cancellationToken = default)
  {
    return await _identityContext
      .Set<Resource>()
      .Include(resource => resource.Scopes)
      .Where(resource => resource.Scopes.Any(scope => scopes.Contains(scope.Name)))
      .ToListAsync(cancellationToken: cancellationToken);
  }
}