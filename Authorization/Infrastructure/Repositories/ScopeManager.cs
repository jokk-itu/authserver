using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class ScopeManager
{
  private readonly IdentityContext _identityContext;

  public ScopeManager(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<IEnumerable<Scope>> ReadScopesAsync(CancellationToken cancellationToken = default)
  {
    return await _identityContext
      .Set<Scope>()
      .ToListAsync();
  }
}
