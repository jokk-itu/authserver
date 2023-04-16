using Domain;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Requests.CreateResource;
using Xunit;

namespace Specs.Validators;
public class CreateResourceValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new[] { ScopeConstants.OpenId },
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.False(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new [] { "invalid_scopes" },
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyScopes_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new List<string>(),
      ResourceName = "test"
    };
    var validator = new CreateResourceValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyResourceName_ExpectErrorResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      Scopes = new[] { ScopeConstants.OpenId },
      ResourceName = string.Empty
    };
    var validator = new CreateResourceValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExistingResourceName_ExpectErrorResult()
  {
    // Arrange
    await IdentityContext
      .Set<Resource>()
      .AddAsync(new Resource
      {
        Id = Guid.NewGuid().ToString(),
        Name = "test",
        Secret = CryptographyHelper.GetRandomString(16)
      });
    await IdentityContext.SaveChangesAsync();

    var command = new CreateResourceCommand
    {
      ResourceName = "test",
      Scopes = new[] { ScopeConstants.OpenId }
    };
    var validator = new CreateResourceValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }
}
