using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RefreshToken;
using Infrastructure.Helpers;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class RedeemRefreshTokenGrantValidatorTests : BaseUnitTest
{
  [Fact]
  public async Task Validate_NullRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var command = new RedeemRefreshTokenGrantCommand
    {
      RefreshToken = null
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_AudienceClaimMismatchWithClientParameter_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = "mismatching_client_id",
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_RevokedStructuredRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
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
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }
  
  [Fact]
  public async Task Validate_ExpiredRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
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
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_RevokedReferenceRefreshToken_InvalidRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
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
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_EmptyGrantType_InvalidGrant()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = string.Empty,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidGrant, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_MaxAgeReachedInAuthorizationGrant_LoginRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.MaxAge = 0;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_InvalidClientSecret_InvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = "invalid_secret",
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_ClientIsUnauthorizedForRefresh_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.Client.GrantTypes.Clear();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_RevokedSession_InvalidSession()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id,
      Scope = scopes.ToString()
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_WithStructuredToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }

  [Fact]
  public async Task Validate_WithReferenceToken_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var identityConfiguration = serviceProvider.GetRequiredService<IdentityConfiguration>();
    identityConfiguration.UseReferenceTokens = true;
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RefreshTokenArguments>>();
    var scopes = new[] { ScopeConstants.OpenId };
    var token = await tokenBuilder.BuildToken(new RefreshTokenArguments
    {
      Scope = scopes.ToString(),
      AuthorizationGrantId = authorizationGrant.Id
    });
    await IdentityContext.SaveChangesAsync();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = token,
      Scope = scopes.ToString()
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
  {
    var refreshGrant = await IdentityContext
        .Set<GrantType>()
        .SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken);

    var client = ClientBuilder
      .Instance()
      .AddGrantType(refreshGrant)
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
      .Build();

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    return authorizationGrant;
  }
}
