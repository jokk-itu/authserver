﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Domain;
using Domain.Constants;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Contracts.GetClientInitialAccessToken;
using WebApp.Contracts.GetScopeInitialAccessToken;
using WebApp.Contracts.PostClient;
using WebApp.Contracts.PostResource;
using WebApp.Contracts.PostScope;
using Xunit;

namespace Specs;
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;

  public HttpClient Client { get; set; }

  protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
  {
      _factory = factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Integration");
      });
      var identityContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
      identityContext.Database.EnsureDeleted();
      identityContext.Database.EnsureCreated();
      Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
      {
        AllowAutoRedirect = false
      });

      BuildScopeAsync("identityprovider:read").GetAwaiter().GetResult();
      BuildResourceAsync("identityprovider:read", "identityprovider").GetAwaiter().GetResult();
  }

  protected async Task<PostScopeResponse> BuildScopeAsync(string scope)
  {
    var getInitialToken = await Client.GetFromJsonAsync<GetScopeInitialAccessToken>("connect/scope/initial-token");
    if (getInitialToken is null)
      throw new Exception("scope initial-token failed");

    var postScopeRequest = new PostScopeRequest
    {
      ScopeName = scope
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/scope/register")
    {
      Content = JsonContent.Create(postScopeRequest)
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getInitialToken.AccessToken);
    var response = await Client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var postScopeResponse = await response.Content.ReadFromJsonAsync<PostScopeResponse>();
    if (postScopeResponse is null)
      throw new Exception();

    return postScopeResponse;
  }

  protected async Task<PostResourceResponse> BuildResourceAsync(string scope, string name)
  {
    var getInitialToken = await Client.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/resource/initial-token");
    if (getInitialToken is null)
      throw new Exception("resource initial-token failed");

    var postResourceRequest = new PostResourceRequest
    {
      Scope = scope,
      ResourceName = name
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/resource/register")
    {
      Content = JsonContent.Create(postResourceRequest)
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getInitialToken.AccessToken);
    var response = await Client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var postResourceResponse = await response.Content.ReadFromJsonAsync<PostResourceResponse>();
    if (postResourceResponse is null)
      throw new Exception();

    return postResourceResponse;
  }

  protected async Task<PostClientResponse> BuildClientAsync(string applicationType, string name)
  {
    var getInitialToken = await Client.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/client/initial-token");
    if (getInitialToken is null)
      throw new Exception("client initial-token failed");

    var postClientRequest = new PostClientRequest
    {
      ApplicationType = applicationType,
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile}",
      ClientName = name,
      GrantTypes = new[] { GrantTypeConstants.AuthorizationCode, GrantTypeConstants.RefreshToken },
      RedirectUris = new[] { "http://localhost:5002/callback" },
      SubjectType = SubjectTypeConstants.Public,
      ResponseTypes = new[] { ResponseTypeConstants.Code }
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/client/register")
    {
      Content = JsonContent.Create(postClientRequest)
    };
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getInitialToken.AccessToken);
    var response = await Client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    var postClientResponse = await response.Content.ReadFromJsonAsync<PostClientResponse>();
    if (postClientResponse is null)
      throw new JsonException();

    return postClientResponse;
  }

  protected async Task<User> BuildUserAsync(string password)
  {
    var userManager = _factory.Services.GetRequiredService<UserManager<User>>();
    var user = FakeBuilder.UserFaker.Generate();
    user.NormalizedEmail = userManager.NormalizeEmail(user.Email);
    user.NormalizedUserName = userManager.NormalizeName(user.UserName);
    var result = await userManager.CreateAsync(user, password);
    if(result.Succeeded)
      return user;

    throw new Exception($"User creation failed: {result}");
  }
}