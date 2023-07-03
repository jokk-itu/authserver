using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
  public IdentityContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
    if (args.Length != 1)
    {
      throw new ArgumentException("connectionstring is missing", nameof(args));
    }

    optionsBuilder.UseSqlite(args[0]);

    return new IdentityContext(optionsBuilder.Options);
  }
}