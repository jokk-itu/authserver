using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure;

public class IdentityContext : DbContext
{
  public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
  { }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}