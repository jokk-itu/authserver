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
  public async Task IsValidAsync_ForceDefaultValues_ExpectCreatedResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidApplicationType_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidClientName_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidRedirectUris_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_EmptyRedirectUris_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidResponseType_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidGrantTypes_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_EmptyGrantTypes_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidContacts_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_EmptyScopes_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidScopes_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidPolicy_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidTos_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidSubjectType_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  public async Task IsValidAsync_InvalidTokenEndpointAuthMethod_ExpectErrorResult()
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
    var validationResult = await validator.IsValidAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }
}
