using Domain.Constants;
using Infrastructure;
using Infrastructure.Factories.TokenFactories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Domain;
using Infrastructure.Extensions;

namespace Specs.Factories;
public class AccessTokenFactoryTests
{
  private readonly IdentityContext _identityContext;

  public AccessTokenFactoryTests()
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
  public async Task GenerateTokenAsync_ExpectAccessToken()
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
      SecretHash = "secret".Sha256(),
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Resource>().AddAsync(identityResource);
    await _identityContext.SaveChangesAsync();

    var identityConfiguration = new IdentityConfiguration
    {
      AccessTokenExpiration = 3600,
      PrivateKeySecret = "wufigbwiubwgub",
      Audience = "identityprovider",
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
    var logger = Mock.Of<ILogger<AccessTokenFactory>>();
    var accessTokenFactory = new AccessTokenFactory(identityConfiguration, fakeJwtBearerOptions.Object, resourceManager, jwkManager, logger);

    // Act
    var token = await accessTokenFactory.GenerateTokenAsync("test", new[] { "openid", "identityprovider" }, "1234");
    var securityToken = accessTokenFactory.DecodeToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains("identityprovider", securityToken!.Audiences);
    Assert.Equal("openid identityprovider", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }
}
