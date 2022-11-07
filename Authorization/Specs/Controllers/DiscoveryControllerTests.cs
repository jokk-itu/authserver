using Contracts.GetJwksDocument;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using WebApp.Contracts.GetDiscoveryDocument;
using Xunit;

namespace Specs.Controllers;

[Collection("Integration")]
public class DiscoveryControllerTests : BaseIntegrationTest
{
  public DiscoveryControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task GetDiscoveryDocumentAsync_ExpectDocument()
  {
    // Arrange

    // Act
    var response = await Client.GetAsync(".well-known/openid-configuration");
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

    // Act
    var response = await Client.GetAsync(".well-known/jwks");
    var document = await response.Content.ReadFromJsonAsync<GetJwksDocumentResponse>();

    // Assert
    Assert.NotNull(document);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}