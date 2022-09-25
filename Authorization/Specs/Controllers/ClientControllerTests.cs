using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Constants;
using Infrastructure.Requests.DeleteClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Contracts.GetClientInitialAccessToken;
using WebApp.Contracts.PostClient;
using Xunit;

namespace Specs.Controllers;
public class ClientControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public ClientControllerTests(WebApplicationFactory<Program> applicationFactory)
  {
    _applicationFactory = applicationFactory;
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostClientAsync_ExpectOk()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();
    var initialTokenResponse = await client.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/client/initial-token");
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/client/register");
    var postClientRequest = new PostClientRequest
    {
      RedirectUris = new[] { "http://localhost:5002/callback" },
      Scope = $"{ScopeConstants.OpenId}",
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      ClientName = "test"
    };
    request.Content = JsonContent.Create(postClientRequest);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initialTokenResponse!.AccessToken);

    // Act
    var response = await client.PostAsJsonAsync("connect/client/register", postClientRequest);
    var postClientResponse = await response.Content.ReadFromJsonAsync<PostClientResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(postClientResponse);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task DeleteClientAsync_ExpectOk()
  {
    // Arrange
    var httpClient = _applicationFactory.CreateClient();
    var initialTokenResponse = await httpClient.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/client/initial-token");
    var postClientRequest = new HttpRequestMessage(HttpMethod.Post, "connect/client/register");
    postClientRequest.Content = JsonContent.Create(new PostClientRequest
    {
      RedirectUris = new[] { "http://localhost:5002/callback" },
      Scope = $"{ScopeConstants.OpenId}",
      GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken },
      ClientName = "test"
    });
    postClientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initialTokenResponse!.AccessToken);
    var postClientResponse = await httpClient.PostAsJsonAsync("connect/client/register", postClientRequest);
    var client = await postClientResponse.Content.ReadFromJsonAsync<PostClientResponse>();

    var deleteClientRequest = new HttpRequestMessage(HttpMethod.Delete, "connect/client/configuration");
    deleteClientRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", client!.RegistrationAccessToken);

    // Act
    var deleteClientResponse = await httpClient.SendAsync(deleteClientRequest);

    // Assert
    Assert.Equal(HttpStatusCode.NoContent, deleteClientResponse.StatusCode);
  }
}