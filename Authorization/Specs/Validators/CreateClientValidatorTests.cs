using Domain;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Requests.CreateClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Xunit;

namespace Specs.Validators;
public class CreateClientValidatorTests
{
  private readonly IdentityContext _identityContext;

  public CreateClientValidatorTests()
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
  public async Task ValidateAsync_ForceDefaultValues_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = string.Empty,
      ResponseTypes = new List<string>(),
      TokenEndpointAuthMethod = string.Empty,
      Contacts = new List<string>(),
      PolicyUri = string.Empty,
      RedirectUris = new[] { "http://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = string.Empty,
      TosUri = string.Empty,
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task IsValid_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidApplicationType_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = "wrong_applicationtype",
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientName_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = string.Empty
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExistingClientName_ExpectErrorResult()
  {
    // Arrange
    await _identityContext
      .Set<Client>()
      .AddAsync(new Client
      {
        Id = Guid.NewGuid().ToString(),
        Name = "test"
      });
    await _identityContext.SaveChangesAsync();

    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidRedirectUris_ExpectErrorResult()
  {
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "invalid_redirecturis" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyRedirectUris_ExpectErrorResult()
  {
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new List<string>(),
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidResponseType_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { "invalid_responsetype" },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidGrantTypes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new[] { "invalid_granttypes" },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyGrantTypes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new List<string>(),
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidContacts_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "invalid_contacts" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new List<string>(),
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "https://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { "invalid_scopes" },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPolicy_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "invalid_policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTos_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "http://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "invalid_tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidSubjectType_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "http://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = "invalid_subject_type",
      TosUri = "http://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTokenEndpointAuthMethod_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateClientCommand
    {
      ApplicationType = ApplicationTypeConstants.Web,
      ResponseTypes = new[] { ResponseTypeConstants.Code },
      TokenEndpointAuthMethod = "invalid_tokenendpointauthmethod",
      Contacts = new[] { "test@mail.dk" },
      PolicyUri = "http://localhost:5002/policy",
      RedirectUris = new[] { "https://localhost:5002/callback" },
      Scopes = new[] { ScopeConstants.OpenId },
      GrantTypes = new [] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "http://localhost:5002/tos",
      ClientName = "test"
    };
    var validator = new CreateClientValidator(_identityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }
}
