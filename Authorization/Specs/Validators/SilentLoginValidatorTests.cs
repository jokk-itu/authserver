using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.IdToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.SilentLogin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class SilentLoginValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidClientId()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var query = new SilentLoginCommand();

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidRedirectUr()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();

    var query = new SilentLoginCommand
    {
      ClientId = client.Id
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidState()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback"
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectInvalidToken()
  {
    //  Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      IdTokenHint = null
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
  public async Task ValidateAsync_NoOpenIdScope_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidScope, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId} nonexistingscope",
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidScope, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidResponseType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      IdTokenHint = idToken
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
  public async Task ValidateAsync_InvalidCodeChallengeMethod()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      IdTokenHint = idToken
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
  public async Task ValidateAsync_InvalidNonce()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      IdTokenHint = idToken
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
  public async Task ValidateAsync_InvalidCodeChallenge()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      IdTokenHint = idToken
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
  public async Task ValidateAsync_UnauthorizedClientForGrantType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    client.GrantTypes.Clear();
    await IdentityContext.SaveChangesAsync();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });
    
    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Nonce = CryptographyHelper.GetRandomString(16),
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnauthorizedClientForRedirectUri()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    client.RedirectUris.Clear();
    await IdentityContext.SaveChangesAsync();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    
    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Nonce = CryptographyHelper.GetRandomString(16),
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }
  
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnauthorizedClientForScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    client.Scopes.Clear();
    await IdentityContext.SaveChangesAsync();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      Nonce = CryptographyHelper.GetRandomString(16),
      IdTokenHint = idToken
    };

    // Act
    var validationResult = await validator.ValidateAsync(query);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidConsent()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var profileScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.Profile);

    client.Scopes.Add(profileScope);
    await IdentityContext.SaveChangesAsync();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5001/callback",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile}",
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
    Assert.Equal(ErrorCode.ConsentRequired, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectExpiredMaxAge()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    authorizationGrant.MaxAge = 0;
    await IdentityContext.SaveChangesAsync();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
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
  public async Task ValidateAsync_ExpectRevokedSession()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();
    var session = authorizationGrant.Session;
    session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
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
  public async Task ValidateAsync_ExpectOk()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var authorizationGrant = client.AuthorizationCodeGrants.Single();

    var validator = serviceProvider.GetRequiredService<IValidator<SilentLoginCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var query = new SilentLoginCommand
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