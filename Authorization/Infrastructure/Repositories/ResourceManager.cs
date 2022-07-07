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

  public async Task<ICollection<IdentityResource>> FindResourcesByScopes(ICollection<string> scopes) =>
      await _context.ResourceScopes
          .Where(rs =>
              scopes.Any(s => s.Equals(rs.ScopeId)))
          .DistinctBy(rs => rs.ResourceId)
          .Select(rs =>
              new IdentityResource { Id = rs.ResourceId })
          .ToListAsync();
}