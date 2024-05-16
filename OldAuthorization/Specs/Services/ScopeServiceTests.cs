using Application;
using Domain.Constants;
using Infrastructure.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Specs.Services;

public class ScopeServiceTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  [InlineData("weather:read")]
  public async Task ValidateScope_EmptyAndNullAndNotContainingOpenId_InvalidScope(string scope)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IScopeService>();

    // Act
    var scopeValidation = await service.ValidateScope(scope, CancellationToken.None);

    // Assert
    Assert.True(scopeValidation.IsError());
    Assert.Equal(ErrorCode.InvalidScope, scopeValidation.ErrorCode);
  }

  [Fact]
  public async Task ValidateScope_NonExistingScoped_InvalidScope()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IScopeService>();

    // Act
    var scopeValidation = await service.ValidateScope($"weather:read {ScopeConstants.OpenId}", CancellationToken.None);

    // Assert
    Assert.True(scopeValidation.IsError());
    Assert.Equal(ErrorCode.InvalidScope, scopeValidation.ErrorCode);
  }

  [Fact]
  public async Task ValidateScope_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<IScopeService>();

    // Act
    var scopeValidation = await service.ValidateScope(ScopeConstants.OpenId, CancellationToken.None);

    // Assert
    Assert.False(scopeValidation.IsError());
  }
}