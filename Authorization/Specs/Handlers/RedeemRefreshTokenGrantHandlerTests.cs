using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.Abstract;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class RedeemRefreshTokenGrantHandlerTests : BaseUnitTest
{
  [Theory]
  [InlineData(null)]
  [InlineData($"{ScopeConstants.OpenId}")]
  public async Task Handle_StructuredRefreshToken_Ok(string requestScope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    var resourceSecret = CryptographyHelper.GetRandomString(32);
    var resource = await GetResource(resourceSecret);
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = $"{ScopeConstants.OpenId}";
    var refreshToken = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes,
      AuthorizationGrantId = authorizationGrant.Id
    });
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = refreshToken,
      GrantType = GrantTypeConstants.RefreshToken,
      Scope = requestScope,
      Resource = new[] { resource.Uri }
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }

  [Theory]
  [InlineData(null)]
  [InlineData($"{ScopeConstants.OpenId}")]
  public async Task Handle_ReferenceRefreshToken_Ok(string requestScope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    var resourceSecret = CryptographyHelper.GetRandomString(32);
    var resource = await GetResource(resourceSecret);
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = $"{ScopeConstants.OpenId}";
    var refreshToken = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes,
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = refreshToken,
      GrantType = GrantTypeConstants.RefreshToken,
      Scope = requestScope,
      Resource = new[] { resource.Uri }
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }

  private async Task<Resource> GetResource(string resourceSecret)
  {
    var resource = ResourceBuilder
      .Instance()
      .AddSecret(resourceSecret)
      .AddScope(await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId))
      .Build();

    await IdentityContext.Set<Resource>().AddAsync(resource);
    await IdentityContext.SaveChangesAsync();
    return resource;
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant(string clientSecret)
  {
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddClaims(await IdentityContext.Set<Claim>().ToArrayAsync())
      .AddScopes(await IdentityContext.Set<Scope>().ToArrayAsync())
      .Build();

    var refreshGrant =
      await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken);

    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddConsentGrant(consentGrant)
      .AddGrantType(refreshGrant)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddRedeemed()
      .Build();

    var authorizationGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddAuthorizationCode(authorizationCode)
      .AddNonce(nonce)
      .AddClient(client)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .AddConsentGrant(consentGrant)
      .Build();

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    return authorizationGrant;
  }
}