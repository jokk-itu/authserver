using Domain;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Infrastructure.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Services;
public class AuthorizationGrantServiceTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateAuthorizationGrant_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var grant = await CreateAuthorizationGrant();
    var service = serviceProvider.GetRequiredService<IAuthorizationGrantService>();
    var arguments = new CreateAuthorizationGrantArguments
    {
      Nonce = CryptographyHelper.GetRandomString(16),
      Client = grant.Client,
      Session = grant.Session,
      CodeChallenge = CryptographyHelper.GetRandomString(43),
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Scope = ScopeConstants.OpenId
    };

    // Act
    var result = await service.CreateAuthorizationGrant(arguments, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Code);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task UpdateAuthorizationGrant_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var grant = await CreateAuthorizationGrant();
    var service = serviceProvider.GetRequiredService<IAuthorizationGrantService>();
    var arguments = new UpdateAuthorizationGrantArguments
    {
      Nonce = CryptographyHelper.GetRandomString(16),
      AuthorizationCodeGrant = grant,
      CodeChallenge = CryptographyHelper.GetRandomString(43),
      CodeChallengeMethod = CodeChallengeMethodConstants.S256,
      Scope = ScopeConstants.OpenId
    };

    // Act
    var result = await service.UpdateAuthorizationGrant(arguments, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.NotNull(result.Code);
  }

  public async Task<AuthorizationCodeGrant> CreateAuthorizationGrant()
  {
    var client = ClientBuilder
      .Instance()
      .Build();

    var grant = AuthorizationCodeGrantBuilder
      .Instance(Guid.NewGuid().ToString())
      .AddClient(client)
      .Build();

    var session = SessionBuilder
      .Instance()
      .AddAuthorizationCodeGrant(grant)
      .Build();

    await IdentityContext.AddAsync(session);
    await IdentityContext.SaveChangesAsync();
    return grant;
  }
}
