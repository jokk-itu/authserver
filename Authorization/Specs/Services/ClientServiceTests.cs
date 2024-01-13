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

public class ClientServiceTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public async Task ValidateRedirectAuthorization_EmptyAndNullState_InvalidRequest(string state)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateRedirectAuthorization(
      string.Empty,
      null,
      state,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateRedirectAuthorization_EmptyClientId_InvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateRedirectAuthorization(
      string.Empty,
      null,
      CryptographyHelper.GetRandomString(16),
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateRedirectAuthorization_EmptyRedirectUriWithMultipleRegisteredRedirectUris_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5002/callback")
      .AddRedirectUri("https://localhost:5002/callback2")
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateRedirectAuthorization(
      client.Id,
      null,
      CryptographyHelper.GetRandomString(16),
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateRedirectAuthorization_InvalidRedirectUriForClient_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5002/callback")
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateRedirectAuthorization(
      client.Id,
      "https://localhost:5002/wrong-callback",
      CryptographyHelper.GetRandomString(16),
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateRedirectAuthorization_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder
      .Instance()
      .AddRedirectUri("https://localhost:5002/callback")
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateRedirectAuthorization(
      client.Id,
      "https://localhost:5002/callback",
      CryptographyHelper.GetRandomString(16),
      CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public async Task IsConsentValid_NullAndEmptyScope_False(string scope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var isConsentValid = await service.IsConsentValid(
      string.Empty,
      string.Empty,
      scope, CancellationToken.None);

    // Assert
    Assert.False(isConsentValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsConsentValid_NullUserIdAndClientId_False()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var isConsentValid = await service.IsConsentValid(
      string.Empty,
      string.Empty,
      ScopeConstants.OpenId, CancellationToken.None);

    // Assert
    Assert.False(isConsentValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsConsentValid_InadequateConsentedScopes_False()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddConsentGrant(consentGrant)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    
    // Act
    var isConsentValid = await service.IsConsentValid(
      client.Id,
      user.Id,
      ScopeConstants.OpenId,
      CancellationToken.None);

    // Assert
    Assert.False(isConsentValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsConsentValid_True()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();
    var scope = await IdentityContext.Set<Scope>().SingleAsync(s => s.Name == ScopeConstants.OpenId);
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddScopes(new [] { scope })
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(16))
      .AddConsentGrant(consentGrant)
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    
    // Act
    var isConsentValid = await service.IsConsentValid(
      client.Id,
      user.Id,
      ScopeConstants.OpenId,
      CancellationToken.None);

    // Assert
    Assert.True(isConsentValid);
  }

  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [Trait("Category", "Unit")]
  public async Task ValidateClientAuthorization_NullAndEmptyScope_InvalidRequest(string scope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateClientAuthorization(
      scope,
      string.Empty,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateClientAuthorization_EmptyClientId_InvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await service.ValidateClientAuthorization(
      ScopeConstants.OpenId,
      string.Empty,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateClientAuthorization_UnauthorizedForAuthorizationCodeGrant_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    var client = ClientBuilder
      .Instance()
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateClientAuthorization(
      ScopeConstants.OpenId,
      client.Id,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateClientAuthorization_UnauthorizedForScope_UnauthorizedClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    var client = ClientBuilder
      .Instance()
      .AddGrantType(await IdentityContext.Set<GrantType>().SingleAsync(gt => gt.Name == GrantTypeConstants.AuthorizationCode))
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateClientAuthorization(
      ScopeConstants.OpenId,
      client.Id,
      CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateClientAuthorization_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IClientService>();

    var client = ClientBuilder
      .Instance()
      .AddGrantType(await IdentityContext.Set<GrantType>().SingleAsync(gt => gt.Name == GrantTypeConstants.AuthorizationCode))
      .AddScope(await IdentityContext.Set<Scope>().SingleAsync(s => s.Name == ScopeConstants.OpenId))
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateClientAuthorization(
      ScopeConstants.OpenId,
      client.Id,
      CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateResource_EmptyClient_ExpectInvalidTarget()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientService = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await clientService.ValidateResources(new List<string>(), string.Empty, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidTarget, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateResource_NonExistingClient_ExpectInvalidTarget()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientService = serviceProvider.GetRequiredService<IClientService>();

    // Act
    var validationResult = await clientService.ValidateResources(new List<string>{"https://weather.authserver.dk"}, string.Empty, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidTarget, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateResource_ExistingClient_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientService = serviceProvider.GetRequiredService<IClientService>();
    var client = ClientBuilder
      .Instance()
      .AddSecret(CryptographyHelper.GetRandomString(16))
      .AddScope(await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId))
      .Build();
    
    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await clientService.ValidateResources(new List<string>{client.ClientUri}, ScopeConstants.OpenId, CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }
}