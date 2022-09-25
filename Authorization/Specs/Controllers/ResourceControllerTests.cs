using Domain.Constants;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebApp.Contracts.GetClientInitialAccessToken;
using WebApp.Contracts.PostClient;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.PostResource;

namespace Specs.Controllers;
public class ResourceControllerTests :  IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public ResourceControllerTests(WebApplicationFactory<Program> applicationFactory)
  {
    _applicationFactory = applicationFactory;
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostResourceAsync_ExpectOk()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();
    var initialTokenResponse = await client.GetFromJsonAsync<GetResourceInitialAccessToken>("connect/resource/initial-token");
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/resource/register");
    var postResourceRequest = new PostResourceRequest
    {
      Scope = $"{ScopeConstants.OpenId}",
      ResourceName = "test"
    };
    request.Content = JsonContent.Create(postResourceRequest);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initialTokenResponse!.AccessToken);

    // Act
    var response = await client.PostAsJsonAsync("connect/resource/register", postResourceRequest);
    var postResourceResponse = await response.Content.ReadFromJsonAsync<PostResourceResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(postResourceResponse);
  }
}
