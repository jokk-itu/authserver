using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.Builders;
using Xunit;

namespace Specs.Validators;
public class SilentLoginValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidToken()
  {
    //  Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginQuery>>();
    var query = new SilentLoginQuery
    {
      ClientId = Guid.NewGuid().ToString(),
      IdTokenHint = null
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidClientId()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdTokenAsync(
      Guid.Empty.ToString(),
      new[] {ScopeConstants.OpenId},
      authorizationGrant.Nonce,
      session.User.Id,
      session.Id.ToString(),
      authorizationGrant.AuthTime);

    var query = new SilentLoginQuery
    {
      ClientId = client.Id,
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.AccessDenied, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectOk()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdTokenAsync(
      client.Id,
      new[] {ScopeConstants.OpenId},
      authorizationGrant.Nonce,
      session.User.Id,
      session.Id.ToString(),
      authorizationGrant.AuthTime);

    var query = new SilentLoginQuery
    {
      ClientId = client.Id,
      IdTokenHint = idToken
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

    var client = ClientBuilder
      .Instance()
      .AddGrantType(grantType)
      .AddRedirect(new RedirectUri { Uri = "https://localhost:5000/callback" })
      .AddScope(openIdScope)
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance()
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

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return client;
  }
}