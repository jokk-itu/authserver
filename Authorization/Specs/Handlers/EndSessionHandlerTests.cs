using System.Net;
using Domain.Enums;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.EndSession;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class EndSessionHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NoActiveSession_ExpectOk()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    authorizationCodeGrant.Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var postLogoutRedirectUri = authorizationCodeGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Id,
      PostLogoutRedirectUri = postLogoutRedirectUri,
      State = CryptographyHelper.GetRandomString(16)
    };

    var handler = serviceProvider.GetRequiredService<IRequestHandler<EndSessionCommand, EndSessionResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_LogoutClient_ExpectOk()
  {
    // Arrange
    var httpClientMock = new Mock<HttpClient>();
    httpClientMock
      .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(It.IsAny<HttpResponseMessage>())
      .Verifiable();

    var httpClientFactoryMock = new Mock<IHttpClientFactory>();
    httpClientFactoryMock
      .Setup(x => x.CreateClient(It.IsAny<string>()))
      .Returns(httpClientMock.Object)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(x =>
    {
      x.AddSingletonMock(httpClientFactoryMock);
    });

    var authorizationCodeGrant = await GetAuthorizationCodeGrant();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Id
    };

    var handler = serviceProvider.GetRequiredService<IRequestHandler<EndSessionCommand, EndSessionResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationCodeGrant()
  {
    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5001/callback")
      .AddPostLogoutRedirectUri("https://localhost:5001/logout-redirect")
      .AddBackChannelLogoutUri("https://localhost:5001/logout")
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .AddScope(openIdScope)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationCodeGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationCodeGrant;
  }
}