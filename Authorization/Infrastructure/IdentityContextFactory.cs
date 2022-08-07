using AuthorizationServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;
public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
  public IdentityContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
    optionsBuilder.UseSqlServer("Server=localhost,1433;Initial Catalog=Identity;Trusted_Connection=False;User ID=sa;Password=Password12!");

    return new IdentityContext(optionsBuilder.Options);
  }
}