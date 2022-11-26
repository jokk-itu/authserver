using System.Net;
using Application;
using Application.Validation;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateScope;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateScopeHandlerTests : BaseUnitTest
{
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

    var handler = new CreateScopeHandler(IdentityContext, fakeValidator.Object, fakeTokenBuilder.Object);
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

    var handler = new CreateScopeHandler(IdentityContext, fakeValidator.Object, fakeTokenBuilder.Object);
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
