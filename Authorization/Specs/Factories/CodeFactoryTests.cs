using Domain;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.TokenFactories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Specs.Helpers;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Specs.Factories;
public class CodeFactoryTests
{
  private readonly IdentityContext _identityContext;

  public CodeFactoryTests()
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
  public async Task GenerateCodeAsync_ExpectCode()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();
    
    var identityConfiguration = new IdentityConfiguration 
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    var codeFactory = new CodeFactory(
      identityConfiguration, 
      serviceProvider.GetRequiredService<IDataProtectionProvider>(), 
      serviceProvider.GetRequiredService<UserManager<User>>());

    // Act
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      "codeChallenge",
      "userId",
      "nonce");

    // Assert
    Assert.NotNull(code);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task DecodeCode_ExpectAuthorizationCode()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      serviceProvider.GetRequiredService<UserManager<User>>());

    // Act
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      "codeChallenge",
      "userId",
      "nonce");

    var authorizationCode = codeFactory.DecodeCode(code);

    // Assert
    Assert.NotNull(code);
    Assert.NotNull(authorizationCode);
    Assert.Equal("test", authorizationCode!.ClientId);
    Assert.Equal("http://localhost:5002/callback", authorizationCode!.RedirectUri);
    Assert.Equal("codeChallenge", authorizationCode!.CodeChallenge);
    Assert.Equal("userId", authorizationCode!.UserId);
    Assert.Equal("nonce", authorizationCode!.Nonce);
    Assert.Contains("openid", authorizationCode!.Scopes);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectTrue()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddScoped<TestManager>();
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    await serviceProvider.GetRequiredService<TestManager>().AddDataAsync();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      userManager);

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();

    // Act
    var user = await userManager.FindByNameAsync("jokk");
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      pkce.CodeChallenge,
      user.Id,
      "nonce");

    var isValid = await codeFactory.ValidateAsync(
      code, 
      "http://localhost:5002/callback", 
      "test", 
      pkce.CodeVerifier);

    // Assert
    Assert.True(isValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_GivenWrongRedirectUri_ExpectFalse()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddScoped<TestManager>();
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    await serviceProvider.GetRequiredService<TestManager>().AddDataAsync();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      userManager);

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();

    // Act
    var user = await userManager.FindByNameAsync("jokk");
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      pkce.CodeChallenge,
      user.Id,
      "nonce");

    var isValid = await codeFactory.ValidateAsync(
      code,
      "http://localhost:5002/wrong-callback",
      "test",
      pkce.CodeVerifier);

    // Assert
    Assert.False(isValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_GivenWrongClientId_ExpectFalse()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddScoped<TestManager>();
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    await serviceProvider.GetRequiredService<TestManager>().AddDataAsync();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      userManager);

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();

    // Act
    var user = await userManager.FindByNameAsync("jokk");
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      pkce.CodeChallenge,
      user.Id,
      "nonce");

    var isValid = await codeFactory.ValidateAsync(
      code,
      "http://localhost:5002/callback",
      "wrong-clientid",
      pkce.CodeVerifier);

    // Assert
    Assert.False(isValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_GivenWrongCodeVerifier_ExpectFalse()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddScoped<TestManager>();
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    await serviceProvider.GetRequiredService<TestManager>().AddDataAsync();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      userManager);

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();

    // Act
    var user = await userManager.FindByNameAsync("jokk");
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      pkce.CodeChallenge,
      user.Id,
      "nonce");

    var isValid = await codeFactory.ValidateAsync(
      code,
      "http://localhost:5002/callback",
      "test",
      "wrong-codeVerifier");

    // Assert
    Assert.False(isValid);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_GivenWrongUserId_ExpectFalse()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddDataProtection();
    services.AddScoped(_ => _identityContext);
    services.AddScoped<TestManager>();
    services.AddIdentityCore<User>()
       .AddRoles<IdentityRole>()
       .AddDefaultTokenProviders()
       .AddEntityFrameworkStores<IdentityContext>();
    var serviceProvider = services.BuildServiceProvider();

    var identityConfiguration = new IdentityConfiguration
    {
      CodeSecret = "suhdbfiuwhfuiwef",
      CodeExpiration = 300
    };

    await serviceProvider.GetRequiredService<TestManager>().AddDataAsync();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    var codeFactory = new CodeFactory(
      identityConfiguration,
      serviceProvider.GetRequiredService<IDataProtectionProvider>(),
      userManager);

    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();

    // Act
    var code = await codeFactory.GenerateCodeAsync(
      "http://localhost:5002/callback",
      new[] { "openid" },
      "test",
      pkce.CodeChallenge,
      "wrong-userId",
      "nonce");

    var isValid = await codeFactory.ValidateAsync(
      code,
      "http://localhost:5002/callback",
      "test",
      pkce.CodeVerifier);

    // Assert
    Assert.False(isValid);
  }
}
