using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.ClientAccessToken;
using Infrastructure.Builders.Token.GrantAccessToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.TokenIntrospection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class TokenIntrospectionValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTokenTypeHint_UnsupportedTokenType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = "invalid_token_type_hint"
    };

    // Act
    var response = await validator.ValidateAsync(query);

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
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken
    };

    // Act
    var response = await validator.ValidateAsync(query);

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
    var authorizationGrant = await GetAuthorizationGrant();
    var client = authorizationGrant.Client;
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      ResourceNames = new [] { "identity-provider" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
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
    var authorizationGrant = await GetAuthorizationGrant();
    var client = authorizationGrant.Client;
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      ResourceNames = new [] { "identity-provider" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientSecret = client.Secret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.InvalidClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_MismatchingClientId_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var client = await GetClient();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      ResourceNames = new [] { "identity-provider" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientId = client.Id,
      ClientSecret = client.Secret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_GrantAccessToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      ResourceNames = new [] { "identity-provider" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ClientAccessToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<ClientAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new ClientAccessTokenArguments
    {
      ClientId = client.Id,
      ResourceNames = new [] { "identity-provider" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientId = client.Id,
      ClientSecret = client.Secret,
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.False(response.IsError());
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
      .AddRedirectUri("https://localhost:5001/callback")
      .AddScope(openIdScope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    return client;
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
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
