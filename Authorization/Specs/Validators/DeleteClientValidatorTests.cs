using Application.Validation;
using Domain;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Requests.DeleteClient;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;
public class DeleteClientValidatorTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_ExpectOkResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = await GetClient();
    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RegistrationTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RegistrationTokenArguments
    {
      Client = client
    });
    await IdentityContext.SaveChangesAsync();

    var command = new DeleteClientCommand(client.Id, token);
    var validator = serviceProvider.GetRequiredService<IValidator<DeleteClientCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command);

    // Assert
    Assert.False(validationResult.IsError());
  }

  private async Task<Client> GetClient()
  {
    var client = ClientBuilder
      .Instance()
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    return client;
  }
}
