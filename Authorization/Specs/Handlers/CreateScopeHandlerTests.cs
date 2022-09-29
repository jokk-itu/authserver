using System.Net;
using Application;
using Application.Validation;
using Infrastructure;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateClient;
using Infrastructure.Requests.CreateScope;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateScopeHandlerTests
{
  private readonly IdentityContext _identityContext;

  public CreateScopeHandlerTests()
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
  public async Task HandleAsync_ValidateFalse_ExpectErrorResult()
  {
    // Arrange
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    var fakeValidator = new Mock<IValidator<CreateScopeCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.BadRequest)
    {
      ErrorCode = ErrorCode.InvalidClientMetadata,
      ErrorDescription = string.Empty
    };
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<CreateScopeCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new CreateScopeHandler(_identityContext, fakeValidator.Object, fakeTokenBuilder.Object);
    var command = new CreateScopeCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task HandleAsync_CreateScope_ExpectCreatedResult()
  {
    // Arrange
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    var fakeValidator = new Mock<IValidator<CreateScopeCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<CreateScopeCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new CreateScopeHandler(_identityContext, fakeValidator.Object, fakeTokenBuilder.Object);
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };

    // Act
    var createdResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(createdResponse.IsError());
    Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
  }
}
