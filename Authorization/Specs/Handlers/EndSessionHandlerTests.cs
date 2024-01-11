using System.Net;
using Domain.Enums;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.IdToken;
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
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var postLogoutRedirectUri = authorizationGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Id,
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

    var authorizationGrant = await GetAuthorizationGrant();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Id
    };

    var handler = serviceProvider.GetRequiredService<IRequestHandler<EndSessionCommand, EndSessionResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Collection(await IdentityContext.Set<Session>().ToListAsync(), session => Assert.True(session.IsRevoked));
    Assert.Collection(await IdentityContext.Set<AuthorizationCodeGrant>().ToListAsync(), grant => Assert.True(grant.IsRevoked));
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
  {
    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var weatherScope = ScopeBuilder
      .Instance()
      .AddName("weather:read")
      .Build();

    var weatherClient = ClientBuilder
      .Instance()
      .AddSecret(CryptographyHelper.GetRandomString(32))
      .AddScope(weatherScope)
      .AddClientUri("https://localhost:5002")
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddSecret(CryptographyHelper.GetRandomString(32))
      .AddRedirectUri("https://localhost:5001/callback")
      .AddPostLogoutRedirectUri("https://localhost:5001/logout-redirect")
      .AddBackChannelLogoutUri("https://localhost:5001/logout")
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .AddScope(openIdScope)
      .AddScope(weatherScope)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var grantAccessToken = GrantAccessTokenBuilder
      .Instance()
      .AddAudience(weatherClient.Name)
      .AddExpiresAt(DateTime.UtcNow.AddHours(1))
      .AddIssuer("https://localhost:5000")
      .AddScope($"{weatherScope.Name} {openIdScope.Name}")
      .Build() as GrantAccessToken;

    var authorizationGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
      .AddToken(grantAccessToken)
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .Build();

    await IdentityContext.AddAsync(weatherClient);
    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationGrant;
  }
}