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
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Xunit;

namespace Specs.Factories;
public class IdTokenFactoryTests
{
  private readonly IdentityContext _identityContext;

  public IdTokenFactoryTests()
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
	public async Task GenerateToken_ExpectIdToken()
	{
    // Arrange
    var identityConfiguration = new IdentityConfiguration 
    {
      IdTokenExpiration = 3600,
      PrivateKeySecret = "wufigbwiubwgub",
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
		var logger = Mock.Of<ILogger<IdTokenFactory>>();
		var idTokenFactory = new IdTokenFactory(identityConfiguration, fakeJwtBearerOptions.Object, jwkManager, logger);

		// Act
		var token = idTokenFactory.GenerateToken("test", new[] { "openid" }, "nonce", "1234");
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
}
