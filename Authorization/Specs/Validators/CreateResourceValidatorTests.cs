using Domain.Constants;
using Infrastructure;
using Infrastructure.Requests.CreateResource;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Specs.Validators;
public class CreateResourceValidatorTests
{
  private readonly IdentityContext _identityContext;

  public CreateResourceValidatorTests()
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
  public async Task IsValidAsync_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new[] { ScopeConstants.OpenId },
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    //Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new [] { "invalid_scopes" },
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_EmptyScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new List<string>(),
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_EmptyResourceName_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new[] { ScopeConstants.OpenId },
      ResourceName = string.Empty
    };
    var validator = new CreateResourceValidator(_identityContext);

    // Act
    var validationResult = await validator.IsValidAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }
}
