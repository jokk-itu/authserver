using AuthorizationServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthorizationServer.Repositories;

public class ResourceManager
{
  private readonly IdentityContext _context;

  public ResourceManager(IdentityContext context)
  {
    _context = context;
  }

  public async Task<ICollection<IdentityResource>> FindResourcesByScopes(ICollection<string> scopes) 
  {
    var resources = new List<IdentityResource>();
    foreach (var scope in scopes) 
    {
      var resourceScope = _context.ResourceScopes.Where(x => x.ScopeId.Equals(scope)).SingleOrDefault();
      if (resourceScope is not null) 
      {
        var resource = await _context.Resources.FindAsync(resourceScope.ResourceId);
        resources.Add(resource);
      }
    }
    return resources;
  }
}