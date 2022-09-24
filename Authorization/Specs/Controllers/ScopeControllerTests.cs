using Domain.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebApp.Contracts.GetResourceInitialAccessToken;
using WebApp.Contracts.GetScopeInitialAccessToken;
using WebApp.Contracts.PostResource;
using WebApp.Contracts.PostScope;
using Xunit;

namespace Specs.Controllers;
public class ScopeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public ScopeControllerTests(WebApplicationFactory<Program> applicationFactory)
  {
    _applicationFactory = applicationFactory;
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostScopeAsync_ExpectOk()
  {
    // Arrange
    var client = _applicationFactory.CreateClient();
    var initialTokenResponse = await client.GetFromJsonAsync<GetScopeInitialAccessToken>("connect/scope/initial-token");
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/scope/register");
    var postScopeRequest = new PostScopeRequest
    {
      ScopeName = "test"
    };
    request.Content = JsonContent.Create(postScopeRequest);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", initialTokenResponse!.AccessToken);

    // Act
    var response = await client.PostAsJsonAsync("connect/scope/register", postScopeRequest);
    var postScopeResponse = await response.Content.ReadFromJsonAsync<PostScopeResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.NotNull(postScopeResponse);
  }
}
