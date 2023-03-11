using System.Net;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using Infrastructure.Requests.DeleteClient;
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

    var tokenBuilder = serviceProvider.GetRequiredService<ITokenBuilder>();
    var tokenDecoder = serviceProvider.GetRequiredService<ITokenDecoder>();

    var token = tokenBuilder.BuildClientRegistrationAccessToken(client.Id);
    var command = new DeleteClientCommand
    {
      ClientRegistrationToken = token
    };
    var handler = new DeleteClientHandler(IdentityContext, tokenDecoder);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
  }
}