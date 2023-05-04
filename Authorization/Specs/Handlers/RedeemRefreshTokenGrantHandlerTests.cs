using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class RedeemRefreshTokenGrantHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddClaims(await IdentityContext.Set<Claim>().ToListAsync())
      .AddScopes(await IdentityContext.Set<Scope>().ToListAsync())
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .AddGrantType(await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken))
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
      .Build();

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var refreshToken = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RefreshToken = refreshToken,
      GrantType = GrantTypeConstants.RefreshToken,
      Scope = scopes.ToString()
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }
}