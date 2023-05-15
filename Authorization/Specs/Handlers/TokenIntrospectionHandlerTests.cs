using System.Net;
using Application;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.ClientAccessToken;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.TokenIntrospection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class TokenIntrospectionHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_InvalidToken_IsInActive()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<TokenIntrospectionQuery, TokenIntrospectionResponse>>();
    var query = new TokenIntrospectionQuery
    {
      Token = "invalid_token",
    };

    // Act
    var response = await handler.Handle(query, cancellationToken: CancellationToken.None);

    // Assert
    Assert.False(response.Active);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Handle_ClientAccessToken_ActiveToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var client = await GetClient();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<ClientAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new ClientAccessTokenArguments
    {
      ClientId = client.Id,
      ResourceNames = new [] {"identity-provider"},
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<TokenIntrospectionQuery, TokenIntrospectionResponse>>();
    var query = new TokenIntrospectionQuery
    {
      Token = token,
      ClientId = client.Id,
      ClientSecret = client.Secret,
      TokenTypeHint = TokenTypeConstants.AccessToken
    };

    // Act
    var response = await handler.Handle(query, cancellationToken: CancellationToken.None);

    // Assert
    Assert.True(response.Active);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Handle_GrantAccessToken_ActiveToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments()
    {
      AuthorizationGrantId = authorizationGrant.Id,
      ResourceNames = new [] {"identity-provider"},
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<TokenIntrospectionQuery, TokenIntrospectionResponse>>();
    var query = new TokenIntrospectionQuery
    {
      Token = token,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      TokenTypeHint = TokenTypeConstants.AccessToken
    };

    // Act
    var response = await handler.Handle(query, cancellationToken: CancellationToken.None);

    // Assert
    Assert.True(response.Active);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  private async Task<Client> GetClient()
  {
    var grantType = await IdentityContext
      .Set<GrantType>()
      .SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);

    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var client = ClientBuilder
      .Instance()
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5001/callback")
      .AddScope(openIdScope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    return client;
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
  {
    var grantType = await IdentityContext
      .Set<GrantType>()
      .SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);

    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var consent = ConsentGrantBuilder
      .Instance()
      .AddScopes(new [] { openIdScope })
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5001/callback")
      .AddScope(openIdScope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .AddConsentGrant(consent)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
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
      .AddConsentGrant(consent)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationGrant;
  }
}
