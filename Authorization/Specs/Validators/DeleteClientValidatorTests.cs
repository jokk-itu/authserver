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
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = string.Empty
    };
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
    var validator = new DeleteClientValidator(IdentityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    //Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_TokenWithoutClientIdScope_ExpectErrorResult()
  {
    // Arrange
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = tokenBuilder.BuildClientInitialAccessToken()
    };
    var validator = new DeleteClientValidator(IdentityContext, tokenDecoder);

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.True(validationResult.IsError());
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_TokenWithInvalidClientId_ExpectErrorResult()
  {
    // Arrange
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
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
    var client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      Name = "test"
    };
    await IdentityContext
      .Set<Client>()
      .AddAsync(client);
    await IdentityContext.SaveChangesAsync();
    var tokenBuilder = ServiceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = ServiceProvider.GetRequiredService<ITokenDecoder>();
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
