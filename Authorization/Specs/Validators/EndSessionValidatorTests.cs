using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.IdToken;
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
  public async Task ValidateAsync_InvalidPostLogoutRedirectUri_ExpectUnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Client.Id,
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
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var postLogoutRedirectUri = authorizationGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Client.Id,
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
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Client.Id,
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
    var authorizationGrant = await GetAuthorizationGrant();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<IdTokenArguments>>();
    var idToken = await tokenBuilder.BuildToken(new IdTokenArguments
    {
      AuthorizationGrantId = authorizationGrant.Id
    });

    var postLogoutRedirectUri = authorizationGrant.Client.RedirectUris
      .Single(x => x.Type == RedirectUriType.PostLogoutRedirectUri).Uri;

    var command = new EndSessionCommand
    {
      IdTokenHint = idToken,
      ClientId = authorizationGrant.Client.Id,
      State = CryptographyHelper.GetRandomString(16),
      PostLogoutRedirectUri = postLogoutRedirectUri
    };

    var validator = serviceProvider.GetRequiredService<IValidator<EndSessionCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }

  private async Task<AuthorizationCodeGrant> GetAuthorizationGrant()
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
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();
    return authorizationGrant;
  }
}
