using Domain;
using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Specs.Helpers.Builders;
using Xunit;

namespace Specs.Builders;
public class TokenBuilderTests : BaseUnitTest
{

  [Fact]
  [Trait("Category", "Unit")]
  public async Task BuildAccessTokenAsync_ExpectAccessToken()
  {
    // Arrange
    var identityScope = new Scope
    {
      Name = "identityprovider:read"
    };

    var identityResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "api", 
      Secret = CryptographyHelper.GetRandomString(32),
      Scopes = new[] { identityScope }
    };
    await IdentityContext.Set<Resource>().AddAsync(identityResource);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
    // Act
    var token = await tokenBuilder.BuildAccessTokenAsync("test", new[] { ScopeConstants.OpenId, identityScope.Name }, "1234", "123");
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains(identityResource.Name, securityToken!.Audiences);
    Assert.Equal($"{ScopeConstants.OpenId} {identityScope.Name}", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task BuildIdToken_ExpectIdToken()
  {
    // Arrange
    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(32))
      .Build();

    await IdentityContext.AddAsync(user);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = await tokenBuilder.BuildIdTokenAsync("test", new[] { ScopeConstants.OpenId }, "nonce", user.Id, "123", DateTime.UtcNow);
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal(user.Id, securityToken!.Subject);
    Assert.Contains("test", securityToken!.Audiences);
    Assert.Equal(ScopeConstants.OpenId, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("nonce", securityToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Nonce).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task BuildRefreshToken_ExpectRefreshToken()
  {
    // Arrange
    var identityScope = new Scope
    {
      Name = "identityprovider:read"
    };
    var identityResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "api",
      Secret = CryptographyHelper.GetRandomString(32),
      Scopes = new[] { identityScope }
    };
    await IdentityContext.Set<Resource>().AddAsync(identityResource);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
   
    // Act
    var token = await tokenBuilder.BuildRefreshTokenAsync("test", new[] { ScopeConstants.OpenId, identityScope.Name }, "1234", "123");
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains(identityResource.Name, securityToken!.Audiences);
    Assert.Equal($"{ScopeConstants.OpenId} {identityScope.Name}", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildClientInitialAccessToken_ExpectInitialAccessToken()
  {
    //Arrange 
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = tokenBuilder.BuildClientInitialAccessToken();
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ClientRegistration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildScopeInitialAccessToken_ExpectInitialAccessToken()
  {
    //Arrange 
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = tokenBuilder.BuildScopeInitialAccessToken();
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ScopeRegistration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildResourceInitialAccessToken_ExpectInitialAccessToken()
  {
    //Arrange 
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
   
    // Act
    var token = tokenBuilder.BuildResourceInitialAccessToken();
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ResourceRegistration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildResourceConfigurationAccessToken_ExpectConfigurationToken()
  {
    //Arrange
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = tokenBuilder.BuildResourceRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("test", securityToken!.Claims.Single(x => x.Type == ClaimNameConstants.ResourceId).Value);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ResourceConfiguration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildClientConfigurationAccessToken_ExpectConfigurationToken()
  {
    //Arrange
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = tokenBuilder.BuildClientRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("test", securityToken!.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ClientConfiguration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildScopeConfigurationAccessToken_ExpectConfigurationToken()
  {
    //Arrange
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();

    // Act
    var token = tokenBuilder.BuildScopeRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeSignedToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("test", securityToken!.Claims.Single(x => x.Type == ClaimNameConstants.ScopeId).Value);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ScopeConfiguration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }
}
