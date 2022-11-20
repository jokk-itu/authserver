using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Specs;
public abstract class BaseUnitTest
{

  protected readonly IdentityContext _identityContext;

  protected BaseUnitTest()
  {
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
      .UseSqlite(connection)
      .Options;
    _identityContext = new IdentityContext(options);
    _identityContext.Database.EnsureCreated();
  }
}