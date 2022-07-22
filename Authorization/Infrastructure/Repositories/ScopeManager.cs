using AuthorizationServer;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;
public class ScopeManager
{
  private readonly IdentityContext _identityContext;

  public ScopeManager(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<IEnumerable<string>> ReadScopesAsync()
  {
    return await _identityContext.Scopes.Select(x => x.Id).ToListAsync();
  }
}
