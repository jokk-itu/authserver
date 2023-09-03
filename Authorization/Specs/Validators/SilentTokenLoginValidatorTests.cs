using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentTokenLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;

public class SilentTokenLoginValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_NullIdToken_LoginRequired()
  {
    //  Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentTokenLoginCommand>>();
    var query = new SilentTokenLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      IdTokenHint = null,
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
  public async Task ValidateAsync_RevokedSession_LoginRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;
    session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentTokenLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentTokenLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      IdTokenHint = idToken
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
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentTokenLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentTokenLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      MaxAge = "20",
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.False(validationResult.IsError());
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
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