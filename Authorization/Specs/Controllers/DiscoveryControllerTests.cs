using Contracts.GetDiscovery;
using Contracts.GetJwksDocument;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
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
    var response = await client.GetAsync(".well-known/openid-configuration");
    var document = await response.Content.ReadFromJsonAsync<GetDiscoveryDocumentResponse>();

    // Assert
    Assert.NotNull(document);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetJwksDocument_ExpectJwks()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();

    // Act
    var response = await client.GetAsync(".well-known/jwks");
    var document = await response.Content.ReadFromJsonAsync<GetJwksDocumentResponse>();

    // Assert
    Assert.NotNull(document);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}