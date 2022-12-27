using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateAuthorizationCodeGrant;
using Infrastructure.Requests.RedeemAuthorizationGrant;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Specs.Helpers;
using Specs.Helpers.Builders;
using Xunit;

namespace Specs.Handlers;
public class RedeemAuthorizationGrantCodeHandlerTests : BaseUnitTest
{
  [Fact]
  public async Task Handle_NotValid()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemAuthorizationCodeGrantCommand>>();
    var result = new ValidationResult(ErrorCode.InvalidRequest,"error", HttpStatusCode.BadRequest);
    validator
      .Setup(x => x.ValidateAsync(It.IsAny<RedeemAuthorizationCodeGrantCommand>(), CancellationToken.None))
      .ReturnsAsync(result);

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddTransient(_ => validator.Object);
    });

    var handler = serviceProvider
      .GetRequiredService<IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>>();

    var command = new RedeemAuthorizationCodeGrantCommand();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(result.ErrorCode, response.ErrorCode);
    Assert.Equal(result.ErrorDescription, response.ErrorDescription);
  }

  [Fact]
  public async Task Handle_Ok()
  {
    // Arrange
    var validator = new Mock<IValidator<RedeemAuthorizationCodeGrantCommand>>();
    var result = new ValidationResult(HttpStatusCode.OK);
    validator.Setup(x => x.ValidateAsync(It.IsAny<RedeemAuthorizationCodeGrantCommand>(), CancellationToken.None))
        .ReturnsAsync(result);

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddTransient(_ => validator.Object);
    });

    var handler = serviceProvider
      .GetRequiredService<IRequestHandler<RedeemAuthorizationCodeGrantCommand, RedeemAuthorizationCodeGrantResponse>>();

    const string uri = "https://localhost:5000/callback";
    var client = ClientBuilder
      .Instance()
      .AddRedirect(new RedirectUri{Uri = uri})
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance()
      .AddClient(client)
      .Build();

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await serviceProvider
        .GetRequiredService<ICodeBuilder>()
        .BuildAuthorizationCodeAsync(authorizationCodeGrant.Id, pkce.CodeChallenge, CodeChallengeMethodConstants.S256, new[] { ScopeConstants.OpenId });

    authorizationCodeGrant.Code = code;

    var session = SessionBuilder
        .Instance()
        .AddAuthorizationCodeGrant(authorizationCodeGrant)
        .Build();

    var user = UserBuilder
        .Instance()
        .AddPassword(CryptographyHelper.GetRandomString(16))
        .AddSession(session)
        .Build();

    await IdentityContext
      .Set<User>()
      .AddAsync(user);

    await IdentityContext.SaveChangesAsync();

    var command = new RedeemAuthorizationCodeGrantCommand
    {
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RedirectUri = uri,
      GrantType = GrantTypeConstants.AuthorizationCode,
      Scope = ScopeConstants.OpenId,
      Code = code,
      CodeVerifier = pkce.CodeVerifier
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }
}
