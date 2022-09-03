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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using Domain;
using Infrastructure.Extensions;

namespace Specs.Factories;
public class RefreshTokenFactoryTests
{
  private readonly IdentityContext _identityContext;

  public RefreshTokenFactoryTests()
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
  public async Task GenerateTokenAsync_ExpectRefreshToken()
  {
    // Arrange
    var identityScope = new Scope
    {
      Name = "identity-provider"
    };
    await _identityContext.Set<Scope>().AddAsync(identityScope);
    await _identityContext.SaveChangesAsync();

    var identityResource = new Resource
    {
      Name = "identity-provider",
      SecretHash = "secret".Sha256(),
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Resource>().AddAsync(identityResource);
    await _identityContext.SaveChangesAsync();

    var identityConfiguration = new IdentityConfiguration
    {
      RefreshTokenExpiration = 2600000,
      PrivateKeySecret = "wufigbwiubwgub",
      Audience = "identity-provider",
      InternalIssuer = "auth-server"
    };
    await JwkManager.GenerateJwkAsync(_identityContext, identityConfiguration, DateTime.UtcNow.AddDays(-7));
    await JwkManager.GenerateJwkAsync(_identityContext, identityConfiguration, DateTime.UtcNow);
    await JwkManager.GenerateJwkAsync(_identityContext, identityConfiguration, DateTime.UtcNow.AddDays(7));
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwkManager = new JwkManager(serviceProvider);

    var openIdConnectConfiguration = new OpenIdConnectConfiguration();
    foreach (var key in jwkManager.Jwks)
    {
      openIdConnectConfiguration.SigningKeys.Add(new RsaSecurityKey(jwkManager.RsaCryptoServiceProvider)
      {
        KeyId = key.KeyId.ToString()
      });
    }

    var fakeConfigurationManager = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
    fakeConfigurationManager
      .Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(openIdConnectConfiguration);

    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = "identity-provider",
      Authority = "auth-server",
      ConfigurationManager = fakeConfigurationManager.Object
    };
    var fakeJwtBearerOptions = new Mock<IOptionsSnapshot<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Get(It.IsAny<string>())).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var logger = Mock.Of<ILogger<RefreshTokenFactory>>();
    var refreshTokenFactory = new RefreshTokenFactory(identityConfiguration, fakeJwtBearerOptions.Object, resourceManager, jwkManager, logger);

    // Act
    var token = await refreshTokenFactory.GenerateTokenAsync("test", new[] { "openid", "identity-provider" }, "1234");
    var securityToken = await refreshTokenFactory.DecodeTokenAsync(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("1234", securityToken!.Subject);
    Assert.Contains("identity-provider", securityToken!.Audiences);
    Assert.Equal("openid identity-provider", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
    Assert.Equal("test", securityToken.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value);
  }
}
