﻿using Domain;
using Domain.Constants;
using Infrastructure.Requests.CreateClient;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Xunit;

namespace Specs.Validators;
public class CreateClientValidatorTests : BaseUnitTest
{
  private readonly CreateClientCommand _command = new()
  {
    ApplicationType = string.Empty,
    ResponseTypes = new List<string>(),
    TokenEndpointAuthMethod = string.Empty,
    Contacts = new List<string>(),
    PolicyUri = string.Empty,
    RedirectUris = new[] { "https://localhost:5002/callback" },
    Scope = string.Empty,
    GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
    SubjectType = string.Empty,
    TosUri = string.Empty,
    ClientName = "test",
    ClientUri = string.Empty,
    DefaultMaxAge = string.Empty,
    LogoUri = string.Empty,
    InitiateLoginUri = string.Empty,
    BackchannelLogoutUri = string.Empty,
    PostLogoutRedirectUris = new List<string>()
  };

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ForceDefaultValues_ExpectCreatedResult()
  {
    // Arrange
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

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
      Scope = $"{ScopeConstants.OpenId}",
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      SubjectType = SubjectTypeConstants.Public,
      TosUri = "https://localhost:5002/tos",
      ClientName = "test",
      ClientUri = "https://localhost:5002",
      DefaultMaxAge = "120",
      LogoUri = "https://gravatar.com/avatar",
      InitiateLoginUri = "https://localhost:5002/login",
      BackchannelLogoutUri = "https://localhost:5002/logout",
      PostLogoutRedirectUris = new[] { "https://localhost:5002" }
    };
    var validator = new CreateClientValidator(IdentityContext);

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
    _command.ApplicationType = "wrong";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientName_ExpectErrorResult()
  {
    // Arrange
    _command.ClientName = string.Empty;
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExistingClientName_ExpectErrorResult()
  {
    // Arrange
    await IdentityContext
      .Set<Client>()
      .AddAsync(new Client
      {
        Id = Guid.NewGuid().ToString(),
        Name = "test"
      });
    await IdentityContext.SaveChangesAsync();

    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidRedirectUris_ExpectErrorResult()
  {
    _command.RedirectUris = new[] { "invalid" };
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyRedirectUris_ExpectErrorResult()
  {
    _command.RedirectUris = Array.Empty<string>();
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidResponseType_ExpectErrorResult()
  {
    // Arrange
    _command.ResponseTypes = new[] { "invalid" };
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidGrantTypes_ExpectErrorResult()
  {
    // Arrange
    _command.GrantTypes = new[] { "invalid" };
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyGrantTypes_ExpectErrorResult()
  {
    // Arrange
    _command.GrantTypes = Array.Empty<string>();
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidContacts_ExpectErrorResult()
  {
    // Arrange
    _command.Contacts = new[] { "invalid" };
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScopes_ExpectErrorResult()
  {
    // Arrange
    _command.Scope = "invalid_scope";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPolicy_ExpectErrorResult()
  {
    // Arrange
    _command.PolicyUri = "invalid";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTos_ExpectErrorResult()
  {
    // Arrange
    _command.TosUri = "invalid";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidSubjectType_ExpectErrorResult()
  {
    // Arrange
    _command.SubjectType = "invalid";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidTokenEndpointAuthMethod_ExpectErrorResult()
  {
    // Arrange
    _command.TokenEndpointAuthMethod = "invalid";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidInitiateLoginUri_ExpectErrorResult()
  {
    // Arrange
    _command.InitiateLoginUri = "invalid_uri";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidLogoUri_ExpectErrorResult()
  {
    // Arrange
    _command.LogoUri = "invalid_uri";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientUri_ExpectErrorResult()
  {
    // Arrange
    _command.ClientUri = "invalid_uri";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidDefaultMaxAgeMinusOne_ExpectErrorResult()
  {
    // Arrange
    _command.DefaultMaxAge = "-2";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidDefaultMaxAge_ExpectErrorResult()
  {
    // Arrange
    _command.DefaultMaxAge = "invalid_number";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidPostLogoutRedirectUris_ExpectErrorResult()
  {
    // Arrange
    _command.PostLogoutRedirectUris = new[] { "invalid_uri" };
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidBackChannelLogoutUri_ExpectErrorResult()
  {
    // Arrange
    _command.BackchannelLogoutUri = "invalid_uri";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidBackChannelLogoutUriWithFragment_ExpectErrorResult()
  {
    // Arrange
    _command.BackchannelLogoutUri = "https://localhost:5002/callback#something";
    var validator = new CreateClientValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(_command);

    // Assert
    Assert.True(validationResult.IsError());
  }
}
