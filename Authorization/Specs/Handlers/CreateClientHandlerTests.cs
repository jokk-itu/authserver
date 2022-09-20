using System.Net;
using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateClientHandlerTests
{
  private readonly IdentityContext _identityContext;

  public CreateClientHandlerTests()
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
  public async Task Handle_ValidateFalse_ExpectErrorResponse()
  {
    // Arrange
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    var fakeValidator = new Mock<IValidator<CreateClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.BadRequest)
    {
      ErrorCode = ErrorCode.InvalidClientMetadata,
      ErrorDescription = string.Empty
    };
    fakeValidator
      .Setup(x => x.IsValidAsync(It.IsAny<CreateClientCommand>()))
      .ReturnsAsync(validationResult);

    var handler = new CreateClientHandler(fakeValidator.Object, _identityContext, fakeTokenBuilder.Object);
    var command = new CreateClientCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  public async Task Handle_CreateClient_ExpectCreatedResponse()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = "web",
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "http://localhost:5002/policy",
      ClientName = "Test",
      RedirectUris = new[] { "http://localhost:5002/callback" },
      SubjectType = SubjectTypeConstants.Public,
      Scopes = new[] { ScopeConstants.OpenId },
      TosUri = "http://localhost:5002/tos"
    };
    
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    const string token = "token";
    fakeTokenBuilder
      .Setup(x => x.BuildClientRegistrationAccessToken(It.IsAny<string>()))
      .Returns(token);

    var fakeValidator = new Mock<IValidator<CreateClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    fakeValidator
      .Setup(x => x.IsValidAsync(It.IsAny<CreateClientCommand>()))
      .ReturnsAsync(validationResult);

    var handler = new CreateClientHandler(fakeValidator.Object, _identityContext, fakeTokenBuilder.Object);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}
