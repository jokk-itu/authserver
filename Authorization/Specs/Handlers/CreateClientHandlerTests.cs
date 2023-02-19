using System.Net;
using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateClient;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateClientHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidateFalse_ExpectErrorResult()
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
      .Setup(x => x.ValidateAsync(It.IsAny<CreateClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new CreateClientHandler(fakeValidator.Object, IdentityContext, fakeTokenBuilder.Object);
    var command = new CreateClientCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateClient_ExpectCreatedResult()
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
      Scope = $"{ScopeConstants.OpenId}",
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
      .Setup(x => x.ValidateAsync(It.IsAny<CreateClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new CreateClientHandler(fakeValidator.Object, IdentityContext, fakeTokenBuilder.Object);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}