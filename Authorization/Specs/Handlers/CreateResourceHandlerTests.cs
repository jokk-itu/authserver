using Application;
using Infrastructure;
using Infrastructure.Builders.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;
using Application.Validation;
using Infrastructure.Requests.CreateResource;
using Xunit;
using Domain.Constants;

namespace Specs.Handlers;
public class CreateResourceHandlerTests
{
  private readonly IdentityContext _identityContext;

  public CreateResourceHandlerTests()
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
  public async Task Handle_ValidateFalse_ExpectErrorResult()
  {
    // Arrange
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    var fakeValidator = new Mock<IValidator<CreateResourceCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.BadRequest)
    {
      ErrorCode = ErrorCode.InvalidClientMetadata,
      ErrorDescription = string.Empty
    };
    fakeValidator
      .Setup(x => x.IsValidAsync(It.IsAny<CreateResourceCommand>()))
      .ReturnsAsync(validationResult);

    var handler = new CreateResourceHandler(_identityContext, fakeValidator.Object, fakeTokenBuilder.Object);
    var command = new CreateResourceCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateResource_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      ResourceName = "api",
      Scopes = new[] { ScopeConstants.OpenId }
    };
    
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    const string token = "token";
    fakeTokenBuilder
      .Setup(x => x.BuildResourceRegistrationAccessToken(It.IsAny<string>()))
      .Returns(token);

    var fakeValidator = new Mock<IValidator<CreateResourceCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    fakeValidator
      .Setup(x => x.IsValidAsync(It.IsAny<CreateResourceCommand>()))
      .ReturnsAsync(validationResult);

    var handler = new CreateResourceHandler(_identityContext, fakeValidator.Object, fakeTokenBuilder.Object);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}
