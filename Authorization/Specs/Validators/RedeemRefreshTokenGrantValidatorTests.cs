using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
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
  public async Task Validate_InvalidRefreshToken()
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
  public async Task Validate_InvalidGrant()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = string.Empty,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, authorizationGrant.Client.Id, scopes, authorizationGrant.Session.User.Id, authorizationGrant.Session.Id)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidGrant, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_InvalidClientIdInRefreshToken()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, "mismatch_client_id", scopes, authorizationGrant.Session.User.Id, authorizationGrant.Session.Id)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.AccessDenied, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_InvalidClientId_ExpectInvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var userId = authorizationGrant.Session.User.Id;
    var sessionId = authorizationGrant.Session.Id;
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = "invalid_id",
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, "invalid_id", scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResponse.ErrorCode);
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
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var userId = authorizationGrant.Session.User.Id;
    var sessionId = authorizationGrant.Session.Id;
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = "invalid_secret",
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, authorizationGrant.Client.Id, scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.Client.GrantTypes.Clear();
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var userId = authorizationGrant.Session.User.Id;
    var sessionId = authorizationGrant.Session.Id;
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, authorizationGrant.Client.Id, scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_InvalidSession()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    authorizationGrant.Session.IsRevoked = true;
    await IdentityContext.SaveChangesAsync();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var userId = authorizationGrant.Session.User.Id;
    var sessionId = authorizationGrant.Session.Id;
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, authorizationGrant.Client.Id, scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var scopes = new[] { ScopeConstants.OpenId };
    var userId = authorizationGrant.Session.User.Id;
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = authorizationGrant.Client.Id,
      ClientSecret = authorizationGrant.Client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(authorizationGrant.Id, authorizationGrant.Client.Id, scopes, userId, authorizationGrant.Session.Id.ToString())
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
  {
    var refreshGrant =
      await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken);

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

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddNonce(nonce)
      .AddAuthorizationCode(authorizationCode)
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

    await IdentityContext.Set<User>().AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    return authorizationCodeGrant;
  }
}
