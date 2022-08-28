using Domain;
using Infrastructure;
using Infrastructure.Extensions;
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
  public async Task ReadResourcesAsync_ExpectTwo()
  {
    // Arrange
    var scope1 = new Scope { Name = "api1" };
    var scope2 = new Scope { Name = "api2" };
    await _identityContext.Set<Scope>().AddRangeAsync(scope1, scope2);
    await _identityContext.Set<Resource>().AddRangeAsync(
      new Resource
      {
        Name = "api1",
        SecretHash = "secret".Sha256(),
        Scopes = new[] { scope1 }
      },
      new Resource
      {
        Name = "api2",
        SecretHash = "secret".Sha256(),
        Scopes = new[] { scope2 }
      },
      new Resource
      {
        Name = "api3",
        SecretHash = "secret".Sha256()
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
