using Domain;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Specs.Repositories;
public class NonceManagerTests
{
  private readonly IdentityContext _identityContext;

  public NonceManagerTests()
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
  public async Task CreateNonceAsync_ExpectTrue()
  {
    // Arrange
    var nonceManager = new NonceManager(_identityContext);

    // Act
    var isCreated = await nonceManager.CreateNonceAsync("nonce");

    // Assert
    Assert.True(isCreated);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadNonceAsync_ExpectNonce()
  {
    // Arrange
    var nonceToBeInserted = new Nonce
    {
      Value = "nonce"
    };
    await _identityContext.Set<Nonce>().AddAsync(nonceToBeInserted);
    await _identityContext.SaveChangesAsync();

    var nonceManager = new NonceManager(_identityContext);

    // Act
    var nonce = await nonceManager.ReadNonceAsync("nonce");

    // Assert
    Assert.NotNull(nonce);
    Assert.Equal(nonceToBeInserted.Id, nonce!.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadNonceAsync_ExpectNull()
  {
    // Arrange
    var nonceManager = new NonceManager(_identityContext);

    // Act
    var nonce = await nonceManager.ReadNonceAsync("nonce");

    // Assert
    Assert.Null(nonce);
  }
}
