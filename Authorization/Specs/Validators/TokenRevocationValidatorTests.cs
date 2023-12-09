using System.Net;
using Application.Validation;
using Application;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Requests.TokenRevocation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Domain.Enums;
using Domain;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Specs.Helpers.EntityBuilders;

namespace Specs.Validators;
public class TokenRevocationValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTokenTypeHint_UnsupportedTokenType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var command = new TokenRevocationCommand
    {
      TokenTypeHint = "invalid_token_type_hint"
    };

    // Act
    var response = await validator.ValidateAsync(command);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.UnsupportedTokenType, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_NullToken_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var command = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.AccessToken
    };

    // Act
    var response = await validator.ValidateAsync(command);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_NullClientSecret_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    var client = authorizationGrant.Client;
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientId = client.Id,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.InvalidClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_NullClientId_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientSecret = clientSecret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.InvalidClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_StructuredToken_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant(CryptographyHelper.GetRandomString(32));
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = await GetClient(clientSecret);
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.RefreshToken,
      Token = token,
      ClientId = client.Id,
      ClientSecret = clientSecret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ReferenceToken_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant(CryptographyHelper.GetRandomString(32));
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = await GetClient(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.RefreshToken,
      Token = token,
      ClientId = client.Id,
      ClientSecret = clientSecret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_StructuredToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.RefreshToken,
      Token = token,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = clientSecret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ReferenceToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenRevocationCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenRevocationCommand
    {
      TokenTypeHint = TokenTypeConstants.RefreshToken,
      Token = token,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = clientSecret
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  private async Task<Client> GetClient(string clientSecret)
  {
    var grantType = await IdentityContext
      .Set<GrantType>()
      .SingleAsync(x => x.Name == GrantTypeConstants.AuthorizationCode);

    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddGrantType(grantType)
      .AddRedirectUri("https://localhost:5001/callback")
      .AddScope(openIdScope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    return client;
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant(string clientSecret)
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
      .AddSecret(clientSecret)
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

    var authorizationGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .AddConsentGrant(consent)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationGrant;
  }
}