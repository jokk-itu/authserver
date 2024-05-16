using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentCookieLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;

public class SilentCookieLoginValidatorTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyUserId_InvalidRequest(string userId)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentCookieLoginCommand>>();
    var query = new SilentCookieLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      UserId = userId
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_RevokedSession_LoginRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    client.AuthorizationCodeGrants.Single().Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentCookieLoginCommand>>();
    var query = new SilentCookieLoginCommand
    {
      UserId = client.ConsentGrants.Single().User.Id,
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentCookieLoginCommand>>();
    var query = new SilentCookieLoginCommand
    {
      UserId = client.ConsentGrants.Single().User.Id,
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      MaxAge = "20"
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.False(validationResult.IsError());
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
      .AddScopes(openIdScope)
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