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
using Infrastructure.Requests.Abstract;
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

  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientAuthentications_ExpectInvalidClient(int count)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = Enumerable.Repeat(new ClientAuthentication(), count).ToList()
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.InvalidClient, response.ErrorCode);
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
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id
        }
      }
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
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientSecret = clientSecret
        }
      }
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
    var authorizationGrant = await GetAuthorizationGrant(CryptographyHelper.GetRandomString(32));
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = await GetClient(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      }
    };

    // Act
    var response = await validator.ValidateAsync(query);

    // Assert
    Assert.True(response.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, response.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ClientHasNoScopesFromToken_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"weather:read"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      }
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
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<GrantAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new GrantAccessTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      }
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
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = await GetClient(clientSecret);
    serviceProvider.GetRequiredService<IdentityConfiguration>().UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<TokenIntrospectionQuery>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<ClientAccessTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new ClientAccessTokenArguments
    {
      ClientId = client.Id,
      Resource = new[] { "https://localhost:5000" },
      Scope = $"{ScopeConstants.OpenId}"
    });
    await IdentityContext.SaveChangesAsync();
    var query = new TokenIntrospectionQuery
    {
      TokenTypeHint = TokenTypeConstants.AccessToken,
      Token = token,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      }
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
      .AddSecret(clientSecret)
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