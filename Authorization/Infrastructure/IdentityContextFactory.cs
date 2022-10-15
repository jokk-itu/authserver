using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
  public IdentityContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
    optionsBuilder.UseSqlite("DataSource=:memory:");
    return new IdentityContext(optionsBuilder.Options);
  }
}