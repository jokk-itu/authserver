using Domain.Constants;
using Domain;
using Infrastructure;
using Infrastructure.Helpers;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;
using Infrastructure.Builders;

namespace Specs.Token;
public class TokenBuilderTests
{
  private readonly IdentityContext _identityContext;

  public TokenBuilderTests()
  {
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    var options = new DbContextOptionsBuilder<IdentityContext>()
      .UseSqlite(connection)
      .Options;
    _identityContext = new IdentityContext(options);
    _identityContext.Database.EnsureCreated();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task BuildAccessTokenAsync_ExpectAccessToken()
  {
    // Arrange
    var identityScope = new Scope
    {
      Name = "identityprovider"
    };
    await _identityContext.Set<Scope>().AddAsync(identityScope);
    await _identityContext.SaveChangesAsync();

    var identityResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "identityprovider", 
      Secret = CryptographyHelper.GetRandomString(32),
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Resource>().AddAsync(identityResource);
    await _identityContext.SaveChangesAsync();

    var identityConfiguration = new IdentityConfiguration
    {
      AccessTokenExpiration = 3600,
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwkManager = new JwkManager(serviceProvider);

    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = "identityprovider",
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptionsSnapshot<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Get(It.IsAny<string>())).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var logger = Mock.Of<ILogger<TokenBuilder>>();
    var tokenBuilder = new TokenBuilder(logger, identityConfiguration, fakeJwtBearerOptions.Object, jwkManager, resourceManager);

    // Act
    var token = await tokenBuilder.BuildAccessTokenAsync("test", new[] { "openid", "identityprovider" }, "1234");
    var securityToken = tokenBuilder.DecodeToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains("identityprovider", securityToken!.Audiences);
    Assert.Equal("openid identityprovider", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void BuildIdToken_ExpectIdToken()
  {
    // Arrange
    var identityConfiguration = new IdentityConfiguration 
    {
      IdTokenExpiration = 3600,
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwkManager = new JwkManager(serviceProvider);

    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = "test",
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptionsSnapshot<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Get(It.IsAny<string>())).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var logger = Mock.Of<ILogger<TokenBuilder>>();
    var idTokenFactory = new TokenBuilder(logger, identityConfiguration, fakeJwtBearerOptions.Object, jwkManager, resourceManager);

    // Act
    var token = idTokenFactory.BuildIdToken("test", new[] { "openid" }, "nonce", "1234");
    var securityToken = idTokenFactory.DecodeToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains("test", securityToken!.Audiences);
    Assert.Equal("openid", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
    Assert.Equal("nonce", securityToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Nonce).Value);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task BuildRefreshToken_ExpectRefreshToken()
  {
    // Arrange
    var identityScope = new Scope
    {
      Name = "identityprovider"
    };
    await _identityContext.Set<Scope>().AddAsync(identityScope);
    await _identityContext.SaveChangesAsync();

    var identityResource = new Resource
    {
      Name = "identityprovider",
      Secret = CryptographyHelper.GetRandomString(32),
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Resource>().AddAsync(identityResource);
    await _identityContext.SaveChangesAsync();

    var identityConfiguration = new IdentityConfiguration
    {
      RefreshTokenExpiration = 2600000,
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwkManager = new JwkManager(serviceProvider);

    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = "identityprovider",
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptionsSnapshot<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Get(It.IsAny<string>())).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var logger = Mock.Of<ILogger<TokenBuilder>>();
    var refreshTokenFactory = new TokenBuilder(logger, identityConfiguration, fakeJwtBearerOptions.Object, jwkManager, resourceManager);

    // Act
    var token = await refreshTokenFactory.BuildRefreshTokenAsync("test", new[] { "openid", "identityprovider" }, "1234");
    var securityToken = refreshTokenFactory.DecodeToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains("identityprovider", securityToken!.Audiences);
    Assert.Equal("openid identityprovider", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }
}
