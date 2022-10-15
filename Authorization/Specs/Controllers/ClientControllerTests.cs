using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Contracts.GetClientInitialAccessToken;
using WebApp.Contracts.PostClient;
using Xunit;

namespace Specs.Controllers;
public class ClientControllerTests : BaseIntegrationTest
{
  public ClientControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task DeleteClientAsync_ExpectOk()
  {
    // Arrange
    var client = await BuildClientAsync(ApplicationTypeConstants.Web, "test");
    var deleteClientRequest = new HttpRequestMessage(HttpMethod.Delete, "connect/client/configuration");
    deleteClientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client!.RegistrationAccessToken);

    // Act
    var deleteClientResponse = await Client.SendAsync(deleteClientRequest);
    deleteClientResponse.EnsureSuccessStatusCode();

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, deleteClientResponse.StatusCode);
  }
}