using Contracts.GetDiscovery;
using Contracts.GetJwksDocument;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace Specs.Controllers;
public class DiscoveryControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public DiscoveryControllerTests(WebApplicationFactory<Program> applicationFactory)
  {
    _applicationFactory = applicationFactory;
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetDiscoveryDocumentAsync_ExpectDocument()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();

    // Act
    var document = await client.GetFromJsonAsync<GetDiscoveryDocumentResponse>(".well-known/openid-configuration");

    // Assert
    Assert.NotNull(document);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetJwksDocument_ExpectJwks()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();

    // Act
    var document = await client.GetFromJsonAsync<GetJwksDocumentResponse>(".well-known/jwks");

    // Assert
    Assert.NotNull(document);
  }
}
