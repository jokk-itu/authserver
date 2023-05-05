using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
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
  public async Task ValidateAsync_NonExistingClientId_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = "invalid_id"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_EmptyRedirectUri_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_EmptyState_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_NoOpenIdScope_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = "api"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidScope, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }
  
  [Fact]
  public async Task ValidateAsync_UnauthorizedScope_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId} api"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidScope, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_UnauthorizedGrantType_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_UnauthorizedRedirectUri_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_ResponseTypeIsNotCode_UnsupportedResponseType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = "invalid_response_type"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.UnsupportedResponseType, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_EmptyCodeChallenge_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_LessThan43CharactersCodeChallenge_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = "lessthanforthythreechars"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }
  
  [Fact]
  public async Task ValidateAsync_InvalidCodeChallengeMethod_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = "invalid_method"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_DuplicateNonce_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    var nonce = NonceBuilder
      .Instance("1")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.AddAsync(nonce);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = nonce.Value
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_MaxAgeNegative_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "-1"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_MaxAgeNotValidNumber_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "invalid_number"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_ConsentIsNull_ConsentRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var client = ClientBuilder
      .Instance()
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "20"
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.ConsentRequired, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_ConsentNotContainsScope_ConsentRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var consent = ConsentGrantBuilder
      .Instance()
      .Build();
    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddConsentGrant(consent)
      .Build();
    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consent)
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(user);
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "20",
      UserId = user.Id
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.True(result.IsError());
    Assert.Equal(ErrorCode.ConsentRequired, result.ErrorCode);
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }

  [Fact]
  public async Task ValidateAsync_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var scope = ScopeBuilder
      .Instance()
      .AddName("api")
      .Build();
    var openid = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
    var grantType = await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);
    var consent = ConsentGrantBuilder
      .Instance()
      .AddScopes(new []{openid, scope})
      .Build();
    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddConsentGrant(consent)
      .Build();
    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consent)
      .AddScope(openid)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5000")
      .Build();
    await IdentityContext.AddAsync(user);
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<CreateAuthorizationGrantCommand>>();
    var command = new CreateAuthorizationGrantCommand
    {
      ClientId = client.Id,
      RedirectUri = "https://localhost:5000",
      State = CryptographyHelper.GetRandomString(16),
      Scope = $"{ScopeConstants.OpenId}",
      ResponseType = ResponseTypeConstants.Code,
      CodeChallenge = ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge,
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Nonce = CryptographyHelper.GetRandomString(16),
      MaxAge = "20",
      UserId = user.Id
    };

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    Assert.False(result.IsError());
    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
  }
}