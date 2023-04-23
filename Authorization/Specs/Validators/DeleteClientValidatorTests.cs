using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.DeleteClient;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Specs.Validators;
public class DeleteClientValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_EmptyToken_ExpectErrorResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = string.Empty
    };
    var tokenDecoder = serviceProvider.GetRequiredService<ITokenDecoder>();
    var validator = new DeleteClientValidator(IdentityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_TokenWithInvalidClientId_ExpectErrorResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = serviceProvider.GetRequiredService<ITokenDecoder>();
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientRegistrationAccessToken("wrong_id")
    };
    var validator = new DeleteClientValidator(IdentityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectOkResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = "test"
    };
    await IdentityContext
      .Set<Client>()
      .AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = serviceProvider.GetRequiredService<ITokenDecoder>();
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientRegistrationAccessToken(client.Id)
    };
    var validator = new DeleteClientValidator(IdentityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }
}
