using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Helpers;
using Infrastructure.Requests.EndSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class EndSessionValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidIdTokenHint_ExpectAccessDenied()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var command = new EndSessionCommand
    {
      IdTokenHint = string.Empty
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.AccessDenied, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientId_ExpectInvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = string.Empty,
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_MismatchingClientIds_ExpectAccessDenied()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      "other_client_id",
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Client.Id,
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.AccessDenied, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPostLogoutRedirectUri_ExpectUnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Client.Id,
      PostLogoutRedirectUri = "https://otherclient/logout-redirect"
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidState_ExpectInvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var postLogoutRedirectUri = authorizationCodeGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Client.Id,
      PostLogoutRedirectUri = postLogoutRedirectUri,
      State = string.Empty
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
    Assert.Equal(HttpStatusCode.BadRequest, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_WithoutPostLogoutRedirectUri_ExpectOk()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Client.Id,
      State = string.Empty,
      PostLogoutRedirectUri = string.Empty
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_WithPostLogoutRedirectUri_ExpectOk()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationCodeGrant = await GetAuthorizationCodeGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var idToken = await tokenBuilder.BuildIdToken(
      authorizationCodeGrant.Id,
      authorizationCodeGrant.Client.Id,
      new[] { $"{ScopeConstants.OpenId}" },
      authorizationCodeGrant.Nonces.Single().Value,
      authorizationCodeGrant.Session.User.Id,
      authorizationCodeGrant.Session.Id,
      DateTime.UtcNow);

    var postLogoutRedirectUri = authorizationCodeGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationCodeGrant.Client.Id,
      State = CryptographyHelper.GetRandomString(16),
      PostLogoutRedirectUri = postLogoutRedirectUri
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationCodeGrant()
  {
    var openIdScope = await IdentityContext
      .Set<Scope>()
      .SingleAsync(x => x.Name == ScopeConstants.OpenId);

    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5001/callback")
      .AddPostLogoutRedirectUri("https://localhost:5001/logout-redirect")
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethod.ClientSecretPost)
      .AddScope(openIdScope)
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
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationCodeGrant;
  }
}
