using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Specs.Repositories;
public class TokenManagerTests
{
  private readonly IdentityContext _identityContext;

  public TokenManagerTests()
	{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
            .UseSqlite(connection)
            .Options;
    _identityContext = new IdentityContext(options);
    _identityContext.Database.EnsureCreated();
  }

  public async Task IsTokenRevokedAsync_ExpectTrue()
  {
    // Arrange

    // Act

    // Assert
  }
}
