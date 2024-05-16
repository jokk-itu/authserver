using System.Net;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Requests.CreateClient;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateClientHandlerTests : BaseUnitTest
{
  private const string Jwks = @"
{
    ""keys"": [
        {
        ""kty"": ""RSA"",
        ""e"": ""AQAB"",
        ""use"": ""sig"",
        ""kid"": ""NTAxZmMxNDMyZDg3MTU1ZGM0MzEzODJhZWI4NDNlZDU1OGFkNjFiMQ"",
        ""alg"": ""RS256"",
        ""n"": ""luZFdW1ynitztkWLC6xKegbRWxky-5P0p4ShYEOkHs30QI2VCuR6Qo4Bz5rTgLBrky03W1GAVrZxuvKRGj9V9-PmjdGtau4CTXu9pLLcqnruaczoSdvBYA3lS9a7zgFU0-s6kMl2EhB-rk7gXluEep7lIOenzfl2f6IoTKa2fVgVd3YKiSGsyL4tztS70vmmX121qm0sTJdKWP4HxXyqK9neolXI9fYyHOYILVNZ69z_73OOVhkh_mvTmWZLM7GM6sApmyLX6OXUp8z0pkY-vT_9-zRxxQs7GurC4_C1nK3rI_0ySUgGEafO1atNjYmlFN-M3tZX6nEcA6g94IavyQ""
        }
    ]
}";

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateClient_ExpectCreatedResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider(services =>
    {
      var fakeTokenBuilder = new Mock<ITokenBuilder<RegistrationTokenArguments>>();
      const string token = "token";
      fakeTokenBuilder
        .Setup(x => x.BuildToken(It.IsAny<RegistrationTokenArguments>()))
        .ReturnsAsync(token);

      services.AddScopedMock(fakeTokenBuilder);
    });

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
      LogoUri = "https://gravatar.com",
      BackchannelLogoutUri = "https://localhost:5002/backchannel-logout",
      PostLogoutRedirectUris = new [] { "https://localhost:5002/postlogout" },
      JwksUri = "https://localhost:5002/.well-known/jwks",
      Jwks = Jwks
    };

    var handler = serviceProvider.GetRequiredService<IRequestHandler<CreateClientCommand, CreateClientResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}