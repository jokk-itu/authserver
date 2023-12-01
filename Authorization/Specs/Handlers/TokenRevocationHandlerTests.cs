using System.Net;
using Application;
using Domain.Enums;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.TokenRevocation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class TokenRevocationHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_StructuredToken_IsRevoked()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();

    var tempToken = await IdentityContext
      .Set<Token>()
      .SingleAsync();
    tempToken.RevokedAt = DateTime.UtcNow;
    await IdentityContext.SaveChangesAsync();

    var handler =
      serviceProvider.GetRequiredService<IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>>();
    var command = new TokenRevocationCommand
    {
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      Token = token,
      TokenTypeHint = TokenTypeConstants.RefreshToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Handle_ReferenceToken_IsRevoked()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments()
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();

    var tempToken = await IdentityContext
      .Set<Token>()
      .SingleAsync();
    tempToken.RevokedAt = DateTime.UtcNow;
    await IdentityContext.SaveChangesAsync();

    var handler =
      serviceProvider.GetRequiredService<IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>>();
    var command = new TokenRevocationCommand
    {
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      Token = token,
      TokenTypeHint = TokenTypeConstants.RefreshToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Handle_StructuredToken_Revoke()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var handler =
      serviceProvider.GetRequiredService<IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>>();
    var command = new TokenRevocationCommand
    {
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      Token = token,
      TokenTypeHint = TokenTypeConstants.RefreshToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);
    var tempToken = await IdentityContext.Set<Token>().SingleAsync();

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(tempToken.RevokedAt);
  }

  [Fact]
  public async Task Handle_ReferenceToken_Revoke()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments()
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var handler =
      serviceProvider.GetRequiredService<IRequestHandler<TokenRevocationCommand, TokenRevocationResponse>>();
    var command = new TokenRevocationCommand
    {
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      Token = token,
      TokenTypeHint = TokenTypeConstants.RefreshToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);
    var tempToken = await IdentityContext.Set<Token>().SingleAsync();

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(tempToken.RevokedAt);
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
