using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.RedeemRefreshTokenGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.Builders;
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
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var clientId = "test";
    var clientSecret = "secret";
    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var sessionId = 1L.ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = null,
      ClientId = clientId,
      ClientSecret = clientSecret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(clientId, scopes, userId, sessionId)
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
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var clientId = "invalid_id";
    var clientSecret = "invalid_secret";
    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var sessionId = 1L.ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = clientId,
      ClientSecret = clientSecret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync("mismatch_client_id", scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_InvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var clientId = "invalid_id";
    var clientSecret = "invalid_secret";
    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var sessionId = 1L.ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = clientId,
      ClientSecret = clientSecret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(clientId, scopes, userId, sessionId)
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
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var client = ClientBuilder
      .Instance()
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance()
      .IsRedeemed()
      .AddClient(client)
      .Build();
    await IdentityContext.Set<AuthorizationCodeGrant>().AddAsync(authorizationCodeGrant);
    await IdentityContext.SaveChangesAsync();

    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var sessionId = 1L.ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(client.Id, scopes, userId, sessionId)
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
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var client = ClientBuilder
      .Instance()
      .AddGrantType(await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken))
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance()
      .IsRedeemed()
      .AddClient(client)
      .Build();
    await IdentityContext.Set<AuthorizationCodeGrant>().AddAsync(authorizationCodeGrant);
    await IdentityContext.SaveChangesAsync();

    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var sessionId = 1L.ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(client.Id, scopes, userId, sessionId)
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResponse.IsError());
    Assert.Equal(ErrorCode.LoginRequired, validationResponse.ErrorCode);
    Assert.Equal(HttpStatusCode.Unauthorized, validationResponse.StatusCode);
  }

  [Fact]
  public async Task Validate_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemRefreshTokenGrantCommand>>();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var client = ClientBuilder
      .Instance()
      .AddGrantType(await IdentityContext.Set<GrantType>().SingleAsync(x => x.Name == GrantTypeConstants.RefreshToken))
      .Build();

    var authorizationCodeGrant = AuthorizationCodeGrantBuilder
      .Instance()
      .IsRedeemed()
      .AddClient(client)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(authorizationCodeGrant)
      .Build();
    await IdentityContext.Set<Session>().AddAsync(session);
    await IdentityContext.SaveChangesAsync();

    var scopes = new[] { "scope" };
    var userId = Guid.NewGuid().ToString();
    var command = new RedeemRefreshTokenGrantCommand
    {
      GrantType = GrantTypeConstants.RefreshToken,
      ClientId = client.Id,
      ClientSecret = client.Secret,
      RefreshToken = await tokenBuilder.BuildRefreshTokenAsync(client.Id, scopes, userId, session.Id.ToString())
    };
    
    // Act
    var validationResponse = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResponse.IsError());
  }
}
