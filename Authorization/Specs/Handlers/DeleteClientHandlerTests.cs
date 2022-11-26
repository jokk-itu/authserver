using Application;
using Moq;
using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.DeleteClient;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Specs.Handlers;

public class DeleteClientHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidateFalse_ExpectErrorResponse()
  {
    // Arrange
    var fakeTokenDecoder = new Mock<ITokenDecoder>();
    var fakeValidator = new Mock<IValidator<DeleteClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.BadRequest)
    {
      ErrorCode = ErrorCode.InvalidClientMetadata,
      ErrorDescription = string.Empty
    };
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<DeleteClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new DeleteClientHandler(IdentityContext, fakeValidator.Object, fakeTokenDecoder.Object);
    var command = new DeleteClientCommand();

    // Act
    var errorResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(errorResponse.ErrorCode, validationResult.ErrorCode);
    Assert.Equal(errorResponse.ErrorDescription, validationResult.ErrorDescription);
    Assert.Equal(errorResponse.StatusCode, validationResult.StatusCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateClient_ExpectCreatedResult()
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

    var token = tokenBuilder.BuildClientRegistrationAccessToken(client.Id);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = token
    };
    var fakeValidator = new Mock<IValidator<DeleteClientCommand>>();
    var validationResult = new ValidationResult(HttpStatusCode.OK);
    fakeValidator
      .Setup(x => x.ValidateAsync(It.IsAny<DeleteClientCommand>(), CancellationToken.None))
      .ReturnsAsync(validationResult);

    var handler = new DeleteClientHandler(IdentityContext, fakeValidator.Object, tokenDecoder);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }
}