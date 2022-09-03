using Domain;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Infrastructure.Extensions;

namespace Specs.Repositories;
public class CodeManagerTests
{
  private readonly IdentityContext _identityContext;

  public CodeManagerTests()
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
  public async Task CreateAuthorizationCodeAsync_Ok()
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

    var codeManager = new CodeManager(_identityContext);

    // Act
    var isCreated = await codeManager.CreateAuthorizationCodeAsync(client, "authorization_code");

    // Assert
    Assert.True(isCreated);
	}

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReedemCodeAsync_Ok()
  {
    // Arrange
    var code = new Code 
    {
      CodeType = CodeType.AuthorizationCode,
      IsRedeemed = false,
      Value = "authorization_code"
    };
    await _identityContext.Set<Code>().AddAsync(code);
    await _identityContext.SaveChangesAsync();

    var codeManager = new CodeManager(_identityContext);

    // Act
    var isRedeemed = await codeManager.ReedemCodeAsync(code);

    // Assert
    Assert.True(isRedeemed);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadCodeAsync_ExpectCode()
  {
    // Arrange
    var codeToBeInserted = new Code
    {
      CodeType = CodeType.AuthorizationCode,
      IsRedeemed = false,
      Value = "authorization_code"
    };
    await _identityContext.Set<Code>().AddAsync(codeToBeInserted);
    await _identityContext.SaveChangesAsync();

    var codeManager = new CodeManager(_identityContext);

    // Act
    var code = await codeManager.ReadCodeAsync("authorization_code");

    // Assert
    Assert.NotNull(code);
    Assert.Equal(codeToBeInserted.Id, code!.Id);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ReadCodeAsync_ExpectNull()
  {
    // Arrange
    var codeManager = new CodeManager(_identityContext);

    // Act
    var code = await codeManager.ReadCodeAsync("authorization_code");

    // Assert
    Assert.Null(code);
  }
}
