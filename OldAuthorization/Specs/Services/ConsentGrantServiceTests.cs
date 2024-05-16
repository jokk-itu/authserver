using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Services;
public class ConsentGrantServiceTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task ValidateConsentedScopes_NullAndEmptyScope_InvalidRequest(string scope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IConsentGrantService>();

    // Act
    var validationResult = await service.ValidateConsentedScopes(
      string.Empty,
      string.Empty,
      scope,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  public async Task ValidateConsentedScopes_EmptyUserIdAndClientId_ConsentRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IConsentGrantService>();

    // Act
    var validationResult = await service.ValidateConsentedScopes(
      string.Empty,
      string.Empty,
      ScopeConstants.OpenId,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.ConsentRequired, validationResult.ErrorCode);
  }

  [Fact]
  public async Task ValidateConsentedScopes_InadequateConsentedScope_ConsentRequired()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IConsentGrantService>();

    var consentGrant = ConsentGrantBuilder
      .Instance()
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .Build();

    await IdentityContext.AddRangeAsync(client, user);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateConsentedScopes(
      user.Id,
      client.Id,
      ScopeConstants.OpenId,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.ConsentRequired, validationResult.ErrorCode);
  }

  [Fact]
  public async Task ValidateConsentedScopes_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IConsentGrantService>();

    var scope = await IdentityContext.Set<Scope>().SingleAsync(s => s.Name == ScopeConstants.OpenId);
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddScopes(scope)
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .Build();

    await IdentityContext.AddRangeAsync(client, user);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateConsentedScopes(
      user.Id,
      client.Id,
      ScopeConstants.OpenId,
      CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }
}
