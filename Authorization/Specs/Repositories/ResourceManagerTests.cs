using Domain;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Specs.Repositories;
public class ResourceManagerTests
{
  private readonly IdentityContext _identityContext;

  public ResourceManagerTests()
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
  public async Task ReadResourcesAsync_ExpectTwo()
  {
    // Arrange
    var scope1 = new Scope { Name = "api1" };
    var scope2 = new Scope { Name = "api2" };
    await _identityContext.Set<Resource>().AddRangeAsync(
      new Resource
      {
        Id = Guid.NewGuid().ToString(),
        Name = "api1",
        Secret = "secret",
        Scopes = new[] { scope1 }
      },
      new Resource
      {
        Id = Guid.NewGuid().ToString(),
        Name = "api2",
        Secret = "secret",
        Scopes = new[] { scope2 }
      },
      new Resource
      {
        Id = Guid.NewGuid().ToString(),
        Name = "api3",
        Secret = "secret"
      });
    await _identityContext.SaveChangesAsync();

    var resourceManager = new ResourceManager(_identityContext);

    // Act
    var resources = await resourceManager.ReadResourcesAsync(new[] { "api1", "api2" });

    // Assert
    Assert.Contains(resources, resource => new[] {"api1", "api2"}.Contains(resource.Name));
    Assert.Equal(2, resources.Count);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadResourcesAsync_ExpectZero()
  {
    // Arrange
    var resourceManager = new ResourceManager(_identityContext);

    //Act
    var resources = await resourceManager.ReadResourcesAsync(new[] { "api1", "api2" });

    // Assert
    Assert.Empty(resources);
  }
}
