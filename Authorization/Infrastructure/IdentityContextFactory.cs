using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
  public IdentityContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
    if (args.Length != 2)
    {
      throw new ArgumentException("datastore provider and connectionstring are missing", nameof(args));
    }

    switch (args[0])
    {
      case "SQLite":
        optionsBuilder.UseSqlite(args[1]);
        break;
      case "SqlServer":
        optionsBuilder.UseSqlServer(args[1]);
        break;
      default:
        throw new ArgumentException("datastore provider is unknown", nameof(args));
    }

    return new IdentityContext(optionsBuilder.Options);
  }
}