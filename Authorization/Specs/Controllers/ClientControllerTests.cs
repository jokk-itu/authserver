using System.Net;
using System.Net.Http.Headers;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Specs.Controllers;

[Collection("Integration")]
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
    const string scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone} {IdentityProviderScope}";
    var client = await BuildAuthorizationGrantClient(ApplicationTypeConstants.Web, "test", scope);
    var deleteClientRequest = new HttpRequestMessage(HttpMethod.Delete, "connect/client/configuration");
    deleteClientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client!.RegistrationAccessToken);

    // Act
    var deleteClientResponse = await Client.SendAsync(deleteClientRequest);
    deleteClientResponse.EnsureSuccessStatusCode();

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, deleteClientResponse.StatusCode);
  }
}