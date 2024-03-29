﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.Abstract;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;

public class RedeemRefreshTokenGrantValidatorTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public async Task Validate_NullRefreshToken_InvalidRefreshToken(string refreshToken)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      RefreshToken = refreshToken
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
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
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = Enumerable.Repeat(new ClientAuthentication(), count).ToList(),
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_AudienceClaimMismatchWithClientParameter_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = "mismatching_client_id",
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_RevokedStructuredRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();

    var jti = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(token).Id);
    var refreshToken = await IdentityContext
      .Set<RefreshToken>()
      .Where(x => x.Id == jti)
      .SingleAsync();

    refreshToken.RevokedAt = DateTime.UtcNow;
    await IdentityContext.SaveChangesAsync();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_ExpiredRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();

    var refreshToken = await IdentityContext
      .Set<RefreshToken>()
      .Where(x => x.Reference == token)
      .SingleAsync();

    refreshToken.ExpiresAt = DateTime.UtcNow.AddHours(-2);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_RevokedReferenceRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();

    var refreshToken = await IdentityContext
      .Set<RefreshToken>()
      .Where(x => x.Reference == token)
      .SingleAsync();

    refreshToken.RevokedAt = DateTime.UtcNow;
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_EmptyGrantType_InvalidGrant()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = string.Empty,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidGrant, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_MaxAgeReachedInAuthorizationGrant_LoginRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    authorizationGrant.MaxAge = 0;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_InvalidClientSecret_InvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = "invalid_secret"
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_ClientIsUnauthorizedForRefresh_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    authorizationGrant.Client.GrantTypes.Clear();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_RevokedSession_InvalidSession()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    authorizationGrant.Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_NullConsentGrant_ConsentRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    authorizationGrant.Client.ConsentGrants.Clear();
    authorizationGrant.Session.User.ConsentGrants.Clear();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.ConsentRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Validate_ScopeExceedsRequestedScope_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, new List<string>());
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = $"{ScopeConstants.OpenId}"
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = $"{ScopeConstants.OpenId}"
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidScope, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Theory]
  [InlineData(null)]
  [InlineData($"{ScopeConstants.OpenId}")]
  [Trait("Category", "Unit")]
  public async Task Validate_WithStructuredToken_Ok(string requestScope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var scopes = new[] { ScopeConstants.OpenId };
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, scopes);
    var weatherClientSecret = CryptographyHelper.GetRandomString(32);
    var weatherClient = await GetWeatherClient(weatherClientSecret);
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = requestScope,
      Resource = new[] { weatherClient.ClientUri }
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }

  [Theory]
  [InlineData(null)]
  [InlineData($"{ScopeConstants.OpenId}")]
  [Trait("Category", "Unit")]
  public async Task Validate_WithReferenceToken_Ok(string requestScope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var scopes = new[] { ScopeConstants.OpenId };
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var authorizationGrant = await GetAuthorizationGrant(clientSecret, scopes);
    var weatherClientSecret = CryptographyHelper.GetRandomString(32);
    var weatherClient = await GetWeatherClient(weatherClientSecret);
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = $"{ScopeConstants.OpenId}",
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = authorizationGrant.Client.Id,
          ClientSecret = clientSecret
        }
      },
      RefreshToken = token,
      Scope = requestScope,
      Resource = new[] { weatherClient.ClientUri }
    };

    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }

  private async Task<Client> GetWeatherClient(string secret)
  {
    var client = ClientBuilder
      .Instance()
      .AddSecret(secret)
      .AddClientUri("https://weather.authserver.dk")
      .AddScopes(
        ScopeBuilder.Instance().AddName("weather:read").Build(),
        await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId))
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    return client;
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant(
    string clientSecret,
    ICollection<string> scopes)
  {
    var consentedScopes = await IdentityContext
      .Set<Scope>()
      .Where(x => scopes.Any(y => y == x.Name))
      .ToArrayAsync();

    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddScopes(consentedScopes)
      .Build();

    var refreshGrant = await IdentityContext
      .Set<GrantType>()
      .SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken);

    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddGrantType(refreshGrant)
      .AddConsentGrant(consentGrant)
      .Build();

    var nonce = NonceBuilder
      .Instance(Guid.NewGuid().ToString())
      .Build();

    var authorizationCode = AuthorizationCodeBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddRedeemed()
      .Build();

    var authorizationGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
      .AddClient(client)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddSession(session)
      .AddConsentGrant(consentGrant)
      .Build();

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    return authorizationGrant;
  }
}