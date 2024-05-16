using System.Net;
using Domain;
using Infrastructure.Builders.Token.Abstractions;
using Infrastructure.Builders.Token.RegistrationToken;
using Infrastructure.Requests.DeleteClient;
using MediatR;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;

namespace Specs.Handlers;

public class DeleteClientHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateClient_ExpectCreatedResult()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var client = ClientBuilder.Instance().Build();
    await IdentityContext
      .Set<Client>()
      .AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder<RegistrationTokenArguments>>();
    var token = await tokenBuilder.BuildToken(new RegistrationTokenArguments
    {
      Client = client
    });
    await IdentityContext.SaveChangesAsync();

    var command = new DeleteClientCommand(client.Id, token);
    var handler = serviceProvider.GetRequiredService<IRequestHandler<DeleteClientCommand, DeleteClientResponse>>();

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }
}