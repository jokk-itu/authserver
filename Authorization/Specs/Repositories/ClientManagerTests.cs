using Domain.Enums;
using Domain;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Domain.Constants;

namespace Specs.Repositories;
public class ClientManagerTests
{
  private readonly IdentityContext _identityContext;

  public ClientManagerTests()
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
  public async Task ReadClientAsync_ExpectClient()
  {
    // Arrange
    var clientToBeInserted = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential
    };
    await _identityContext.Set<Client>().AddAsync(clientToBeInserted);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var client = await clientManager.ReadClientAsync("test");

    // Assert
    Assert.NotNull(client);
    Assert.Equal(clientToBeInserted.Id, client!.Id);
  }

  [Fact]
  public async Task ReadClientAsync_ExpectNull()
  {
    // Arrange
    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var client = await clientManager.ReadClientAsync("test");

    // Assert
    Assert.Null(client);
  }

  [Fact]
  public async Task Login_ExpectTrue()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isLoggedIn = clientManager.Login("secret", client);

    // Assert
    Assert.True(isLoggedIn);
  }

  [Fact]
  public async Task Login_ExpectFalse()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isLoggedIn = clientManager.Login("fake_secret", client);

    // Assert
    Assert.False(isLoggedIn);
  }

  [Fact]
  public async Task IsAuthorizedRedirectUris_ExpectFalse()
  {
    // Arrange
    var redirectUri = new RedirectUri 
    {
      Uri = "http://localhost:50000/callback"
    };
    await _identityContext.Set<RedirectUri>().AddAsync(redirectUri);

    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      RedirectUris = new List<RedirectUri> { redirectUri }
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedRedirectUris(client, new[] { "http://localhost:50000/callback/not/correct" });

    // Assert
    Assert.False(isAuthorized);
  }

  [Fact]
  public async Task IsAuthorizedRedirectUris_ExpectTrue()
  {
    // Arrange
    var redirectUri = new RedirectUri
    {
      Uri = "http://localhost:50000/callback"
    };
    await _identityContext.Set<RedirectUri>().AddAsync(redirectUri);

    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      RedirectUris = new List<RedirectUri> { redirectUri }
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedRedirectUris(client, new[] { "http://localhost:50000/callback" });

    // Assert
    Assert.True(isAuthorized);
  }

  [Fact]
  public async Task IsAuthorizedGrants_ExpectFalse()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      Grants = await _identityContext.Set<Grant>().Where(x => x.Name == GrantConstants.AuthorizationCode).ToListAsync()
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedGrants(client, new[] { "some_grant" });

    // Assert
    Assert.False(isAuthorized);
  }

  [Fact]
  public async Task IsAuthorizedGrants_ExpectTrue()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      Grants = await _identityContext.Set<Grant>().Where(x => x.Name == GrantConstants.AuthorizationCode).ToListAsync()
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedGrants(client, new[] { GrantConstants.AuthorizationCode });

    // Assert
    Assert.True(isAuthorized);
  }

  [Fact]
  public async Task IsAuthorizedScopes_ExpectFalse()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedScopes(client, new[] { "some_scope" });

    // Assert
    Assert.False(isAuthorized);
  }

  [Fact]
  public async Task IsAuthorizedScopes_ExpectTrue()
  {
    // Arrange
    var client = new Client
    {
      Name = "test",
      SecretHash = "secret".Sha256(),
      ClientProfile = ClientProfile.WebApplication,
      ClientType = ClientType.Confidential,
      Scopes = await _identityContext.Set<Scope>().ToListAsync()
    };
    await _identityContext.Set<Client>().AddAsync(client);
    await _identityContext.SaveChangesAsync();

    var clientManager = new ClientManager(_identityContext, Mock.Of<ILogger<ClientManager>>());

    // Act
    var isAuthorized = clientManager.IsAuthorizedScopes(client, new[] { ScopeConstants.Profile, ScopeConstants.OpenId });

    // Assert
    Assert.True(isAuthorized);
  }
}
