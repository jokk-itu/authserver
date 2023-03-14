using System.Net;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Requests.CreateScope;
using Moq;
using Xunit;

namespace Specs.Handlers;
public class CreateScopeHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task HandleAsync_CreateScope_ExpectCreatedResult()
  {
    // Arrange
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    var handler = new CreateScopeHandler(IdentityContext, fakeTokenBuilder.Object);
    var command = new CreateScopeCommand
    {
      ScopeName = "test"
    };

    // Act
    var createdResponse = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.False(createdResponse.IsError());
    Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
  }
}
