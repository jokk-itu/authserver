using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentLogin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Handlers;
public class SilentLoginHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExpectInvalidValidation()
  {
    // Arrange
    var validationResult = new ValidationResult(ErrorCode.InvalidRequest, "invalid request", HttpStatusCode.BadRequest);
    var validator = new Mock<IValidator<SilentLoginCommand>>();
    validator
      .Setup(x => x.ValidateAsync(It.IsAny<SilentLoginCommand>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(validationResult)
      .Verifiable();

    var serviceProvider = BuildServiceProvider(services =>
    {
      services.AddScopedMock(validator);
    });

    var handler = serviceProvider.GetRequiredService<IRequestHandler<SilentLoginCommand, SilentLoginResponse>>();

    // Act
    var response = await handler.Handle(new SilentLoginCommand(), CancellationToken.None);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(validationResult.ErrorCode, response.ErrorCode);
    Assert.Equal(validationResult.ErrorDescription, response.ErrorDescription);
    validator.Verify();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var handler = serviceProvider.GetRequiredService<IRequestHandler<SilentLoginCommand, SilentLoginResponse>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdTokenAsync(
      authorizationGrant.Id,
      client.Id,
      new[] {ScopeConstants.OpenId},
      authorizationGrant.Nonces.Single().Id,
      authorizationGrant.Session.User.Id,
      authorizationGrant.Session.Id,
      authorizationGrant.AuthTime);

    var command = new SilentLoginCommand
    {
      ClientId = client.Id,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Scope = $"{ScopeConstants.OpenId}",
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      ResponseType = ResponseTypeConstants.Code,
      IdTokenHint = idToken
    };

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(response.IsError());
  }

  private async Task<Client> GetClient()
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
      .AddRedirect(new RedirectUri { Uri = "https://localhost:5001/callback" })
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
      .AddConsentGrant(consent)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return client;
  }
}