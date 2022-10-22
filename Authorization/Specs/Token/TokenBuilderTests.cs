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
using Infrastructure.Decoders;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
      Name = "identityprovider:read"
    };
    await _identityContext.Set<Scope>().AddAsync(identityScope);
    await _identityContext.SaveChangesAsync();

    var identityResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "api", 
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
      Audience = identityResource.Name,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = await tokenBuilder.BuildAccessTokenAsync("test", new[] { ScopeConstants.OpenId, identityScope.Name }, "1234", "123");
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var user = new User
    {
      Address = "Beaker Street",
      Birthdate = DateTime.Now,
      Email = "Test@mail.dk",
      FirstName = "John",
      LastName = "Doe",
      Locale = "en-GB",
      PhoneNumber = "00000000",
      UserName = "john"
    };
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
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    await userManager.CreateAsync(user);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = await tokenBuilder.BuildIdTokenAsync("test", new[] { ScopeConstants.OpenId }, "nonce", user.Id, "123");
    var securityToken = tokenDecoder.DecodeToken(token);

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
    await _identityContext.Set<Scope>().AddAsync(identityScope);
    await _identityContext.SaveChangesAsync();

    var identityResource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Name = "api",
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
      Audience = identityResource.Name,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = await tokenBuilder.BuildRefreshTokenAsync("test", new[] { ScopeConstants.OpenId, identityScope.Name }, "1234", "123");
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildClientInitialAccessToken();
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildScopeInitialAccessToken();
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildResourceInitialAccessToken();
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildResourceRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildClientRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeToken(token);

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
    var identityConfiguration = new IdentityConfiguration
    { 
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
      Audience = AudienceConstants.IdentityProvider,
      Authority = "auth-server"
    };
    var fakeJwtBearerOptions = new Mock<IOptions<JwtBearerOptions>>();
    fakeJwtBearerOptions.Setup(x => x.Value).Returns(jwtBearerOptions);
    var resourceManager = new ResourceManager(_identityContext);
    var userStore = new UserStore<User>(_identityContext);
    var userManager = new UserManager<User>(userStore, null, null, null, null, null, null, null, null);
    var tokenBuilder = new TokenBuilder(identityConfiguration, jwkManager, resourceManager, userManager);
    var tokenDecoder = new TokenDecoder(Mock.Of<ILogger<TokenDecoder>>(), fakeJwtBearerOptions.Object, jwkManager);

    // Act
    var token = tokenBuilder.BuildScopeRegistrationAccessToken("test");
    var securityToken = tokenDecoder.DecodeToken(token);

    // Assert
    Assert.NotEmpty(token);
    Assert.NotNull(securityToken);
    Assert.Equal("test", securityToken!.Claims.Single(x => x.Type == ClaimNameConstants.ScopeId).Value);
    Assert.Contains(AudienceConstants.IdentityProvider, securityToken!.Audiences);
    Assert.Equal(ScopeConstants.ScopeConfiguration, securityToken.Claims.Single(x => x.Type == ClaimNameConstants.Scope).Value);
  }
}
