using Domain;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Builders;
using Infrastructure.Decoders;
using Infrastructure.Helpers;
using Infrastructure.Repositories;
using Infrastructure.Requests.DeleteClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Specs.Validators;
public class DeleteClientValidatorTests
{
  private readonly IdentityContext _identityContext;

  public DeleteClientValidatorTests()
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
  public async Task ValidateAsync_EmptyToken_ExpectErrorResult()
  {
    // Arrange
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = string.Empty
    };
    var identityConfiguration = new IdentityConfiguration
    {
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var jwkManager = new JwkManager(serviceProvider);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);
    var validator = new DeleteClientValidator(_identityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_TokenWithoutClientIdScope_ExpectErrorResult()
  {
    // Arrange
    var identityConfiguration = new IdentityConfiguration
    {
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var jwkManager = new JwkManager(serviceProvider);
    var resourceManager = new ResourceManager(_identityContext);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientInitialAccessToken()
    };
    var validator = new DeleteClientValidator(_identityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_TokenWithInvalidClientId_ExpectErrorResult()
  {
    // Arrange
    var identityConfiguration = new IdentityConfiguration
    {
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var jwkManager = new JwkManager(serviceProvider);
    var resourceManager = new ResourceManager(_identityContext);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientRegistrationAccessToken("wrong_id")
    };
    var validator = new DeleteClientValidator(_identityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectOkResult()
  {
    // Arrange
    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = "test"
    };
    await _identityContext
      .Set<Client>()
      .AddAsync(client);
    await _identityContext.SaveChangesAsync();
    var identityConfiguration = new IdentityConfiguration
    {
      PrivateKeySecret = CryptographyHelper.GetRandomString(32),
      InternalIssuer = "auth-server"
    };
    var serviceProvider = new ServiceCollection()
      .AddScoped(_ => _identityContext)
      .AddSingleton(_ => identityConfiguration)
      .BuildServiceProvider();
    var jwtBearerOptions = new JwtBearerOptions
    {
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var jwkManager = new JwkManager(serviceProvider);
    var resourceManager = new ResourceManager(_identityContext);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientRegistrationAccessToken(client.Id)
    };
    var validator = new DeleteClientValidator(_identityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }
}
