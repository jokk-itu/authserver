using Infrastructure.Builders.Abstractions;
using Moq;
using System.Net;
using Infrastructure.Requests.CreateResource;
using Xunit;
using Domain.Constants;

namespace Specs.Handlers;
public class CreateResourceHandlerTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_CreateResource_ExpectCreatedResult()
  {
    // Arrange
    var command = new CreateResourceCommand
    {
      ResourceName = "api",
      Scopes = new[] { ScopeConstants.OpenId }
    };
    
    var fakeTokenBuilder = new Mock<ITokenBuilder>();
    const string token = "token";
    fakeTokenBuilder
      .Setup(x => x.BuildResourceRegistrationAccessToken(It.IsAny<string>()))
      .Returns(token);

    var handler = new CreateResourceHandler(IdentityContext, fakeTokenBuilder.Object);

    // Act
    var response = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
  }
}
