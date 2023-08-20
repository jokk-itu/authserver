using Application;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Services;

public class NonceServiceTests : BaseUnitTest
{
  [Theory]
  [InlineData("")]
  [InlineData(null)]
  public async Task ValidateNonce_NullAndEmptyNonce_InvalidRequest(string nonce)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<INonceService>();

    // Act
    var validationResult = await service.ValidateNonce(nonce, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  public async Task ValidateNonce_NonUniqueNonce_InvalidRequest()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<INonceService>();

    var nonce = NonceBuilder
      .Instance(CryptographyHelper.GetRandomString(16))
      .Build();

    await IdentityContext.AddAsync(nonce);
    await IdentityContext.SaveChangesAsync();

    // Act
    var validationResult = await service.ValidateNonce(nonce.Value, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidRequest, validationResult.ErrorCode);
  }

  [Fact]
  public async Task ValidateNonce_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var service = serviceProvider.GetRequiredService<INonceService>();

    // Act
    var validationResult = await service.ValidateNonce(Guid.NewGuid().ToString(), CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }
}