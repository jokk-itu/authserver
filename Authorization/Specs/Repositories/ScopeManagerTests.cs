using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Domain;
using Infrastructure.Repositories;
using Xunit;

namespace Specs.Repositories;
public class ScopeManagerTests
{
  private readonly IdentityContext _identityContext;

	public ScopeManagerTests()
	{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
            .UseSqlite(connection)
            .Options;
    _identityContext = new IdentityContext(options);
    _identityContext.Database.EnsureCreated();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadScopesAsync_GetTwoScopes()
  {
    //Arrange
    _identityContext.Set<Scope>().RemoveRange(await _identityContext.Set<Scope>().ToListAsync());
    await _identityContext
      .Set<Scope>()
      .AddRangeAsync(
        new Scope 
        {
          Name = "api2"
        },
        new Scope 
        {
          Name = "api3"
        });

    await _identityContext.SaveChangesAsync();

    var scopeManager = new ScopeManager(_identityContext);

    //Act
    var scopes = await scopeManager.ReadScopesAsync();

    //Assert
    Assert.Equal(2, scopes.Count());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadScopesAsync_GetZeroScopes()
  {
    //Arrange
    _identityContext.Set<Scope>().RemoveRange(await _identityContext.Set<Scope>().ToListAsync());
    await _identityContext.SaveChangesAsync();
    var scopeManager = new ScopeManager(_identityContext);

    //Act
    var scopes = await scopeManager.ReadScopesAsync();

    //Assert
    Assert.Empty(scopes);
  }
}
