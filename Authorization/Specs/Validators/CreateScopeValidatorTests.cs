using Domain;
using Infrastructure;
using Infrastructure.Requests.CreateScope;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Specs.Validators;
public class CreateScopeValidatorTests
{
  private readonly IdentityContext _identityContext;

  public CreateScopeValidatorTests()
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
  public async Task IsValidAsync_InvalidScopeName_ExpectErrorResult()
  {
    // Arrange
    await _identityContext.Set<Scope>().AddAsync(new Scope
    {
      Name = "test"
    });
    await _identityContext.SaveChangesAsync();
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };
    var validator = new CreateScopeValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsValidAsync_EmptyScopeName_ExpectErrorResult()
  {
    var command = new CreateScopeCommand
    {
      ScopeName = string.Empty
    };
    var validator = new CreateScopeValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsValidAsync_ExpectCreatedResult()
  {
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };
    var validator = new CreateScopeValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }
}
