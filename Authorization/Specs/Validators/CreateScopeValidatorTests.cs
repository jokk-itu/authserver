using Domain;
using Infrastructure.Requests.CreateScope;
using Xunit;

namespace Specs.Validators;
public class CreateScopeValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidScopeName_ExpectErrorResult()
  {
    // Arrange
    await IdentityContext.Set<Scope>().AddAsync(new Scope
    {
      Name = "test"
    });
    await IdentityContext.SaveChangesAsync();
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };
    var validator = new CreateScopeValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyScopeName_ExpectErrorResult()
  {
    var command = new CreateScopeCommand
    {
      ScopeName = string.Empty
    };
    var validator = new CreateScopeValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectCreatedResult()
  {
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };
    var validator = new CreateScopeValidator(IdentityContext);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }
}
