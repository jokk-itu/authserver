using System.Net;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Requests.CreateClient;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateClientHandlerTests : BaseUnitTest
{
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
      PolicyUri = "https://localhost:5002/policy",
      ClientName = "Test",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      SubjectType = SubjectTypeConstants.Public,
      Scope = $"{ScopeConstants.OpenId}",
      TosUri = "https://localhost:5002/tos",
      ClientUri = "https://localhost:5002",
      DefaultMaxAge = "120",
      InitiateLoginUri = "https://localhost:5002/login",
      LogoUri = "https://gravatar.com/avatar"
    };
    
    var fakeTokenBuilder = new Mock<ITokenBuilder<RegistrationTokenArguments>>();
    const string token = "token";
    fakeTokenBuilder
      .Setup(x => x.BuildToken(It.IsAny<RegistrationTokenArguments>()))
      .ReturnsAsync(token);

    var handler = new CreateClientHandler(IdentityContext, fakeTokenBuilder.Object);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}