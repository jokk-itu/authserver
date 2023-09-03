using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateAuthorizationGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class CreateAuthorizationGrantValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnauthorizedForRedirect_BadRequest()
  {
    //  Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = string.Empty,
      RedirectUri = string.Empty,
      State = string.Empty
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyNonce_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Nonce = string.Empty
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyScope_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Nonce = CryptographyHelper.GetRandomString(16),
      Scope = string.Empty
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidResponseType_UnsupportedResponseType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      Nonce = CryptographyHelper.GetRandomString(16),
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnsupportedResponseType, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidCodeChallengeMethod_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      Nonce = CryptographyHelper.GetRandomString(16)
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
  public async Task ValidateAsync_InvalidCodeChallenge_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
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
  public async Task ValidateAsync_UnauthorizedGrantTypeAndScope_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    client.GrantTypes.Clear();
    client.Scopes.Clear();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Nonce = CryptographyHelper.GetRandomString(16)
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }
 
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InadequateConsent_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var profileScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.Profile);

    client.Scopes.Add(profileScope);
    await IdentityContext.SaveChangesAsync();
    var userId = client.ConsentGrants.Single().User.Id;
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var query = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      UserId = userId
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidMaxAge_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var userId = client.ConsentGrants.Single().User.Id;
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "invalid_number",
      UserId = userId
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var userId = client.ConsentGrants.Single().User.Id;
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "20",
      UserId = userId
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.False(result.IsError());
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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