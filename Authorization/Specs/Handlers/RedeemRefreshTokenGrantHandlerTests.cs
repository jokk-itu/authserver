using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
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
  public async Task Handle_ValidateError()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemRefreshTokenGrantCommand>>();
    validator
      .Setup(x => 
        x.ValidateAsync(It.IsAny<RedeemRefreshTokenGrantCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult(ErrorCode.LoginRequired, "error", HttpStatusCode.Unauthorized));

    var serviceProvider = BuildServiceProvider(services => 
      services.AddTransient(_ => validator.Object));

    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(response.IsError());
  }

  [Fact]
  public async Task Handle_TokenDecodeError()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemRefreshTokenGrantCommand>>();
    validator
      .Setup(x => 
        x.ValidateAsync(It.IsAny<RedeemRefreshTokenGrantCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ValidationResult(HttpStatusCode.OK));

    var serviceProvider = BuildServiceProvider(services => 
      services.AddTransient(_ => validator.Object));

    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      RefreshToken = string.Empty,
    };
    
    // Act & Assert
    await Assert.ThrowsAsync<SecurityTokenException>(() => handler.Handle(command, CancellationToken.None));
  }

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

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddAuthorizationCode(authorizationCode)
      .AddNonce(nonce)
      .AddClient(client)
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

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var refreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationCodeGrant.Id, client.Id, scopes, user.Id, session.Id);
    var handler = serviceProvider.GetRequiredService<IRequestHandler<RedeemRefreshTokenGrantCommand, RedeemRefreshTokenGrantResponse>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RefreshToken = refreshToken,
      GrantType = GrantTypeConstants.RefreshToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }
}