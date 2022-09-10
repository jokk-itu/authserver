using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
  public IdentityContext CreateDbContext(string[] args)
  {
    if (args.Length == 0)
      throw new ArgumentException("First argument must be an SqlConnection string");

    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
    optionsBuilder.UseSqlServer(args[0]);

    return new IdentityContext(optionsBuilder.Options);
  }
}