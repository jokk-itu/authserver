﻿using System.Net.Http.Json;
using Domain;
using Domain.Constants;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using WebApp.Contracts.PostClient;
using WebApp.Contracts.PostResource;
using WebApp.Contracts.PostScope;
using Xunit;

namespace Specs;
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _factory;

  protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
  {
      _factory = factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Integration");
      });
  }

  protected HttpClient GetHttpClient()
  {
    return _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false
    });
  }

  protected async Task CreateDatabase()
  {
    var identityContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<IdentityContext>();
    await identityContext.Database.EnsureDeletedAsync();
    await identityContext.Database.EnsureCreatedAsync();
  }

  protected async Task CreateIdentityProviderResource()
  {
    await BuildResource(ScopeConstants.UserInfo, "identityprovider");
  }

  protected async Task<PostScopeResponse> BuildScope(string scope)
  {
    var postScopeRequest = new PostScopeRequest
    {
      ScopeName = scope
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/scope/register")
    {
      Content = JsonContent.Create(postScopeRequest)
    };
    var response = await GetHttpClient().SendAsync(request);
    response.EnsureSuccessStatusCode();
    var postScopeResponse = await response.Content.ReadFromJsonAsync<PostScopeResponse>();
    return postScopeResponse!;
  }

  protected async Task<PostResourceResponse> BuildResource(string scope, string name)
  {
    var postResourceRequest = new PostResourceRequest
    {
      Scope = scope,
      ResourceName = name
    };
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/resource/register")
    {
      Content = JsonContent.Create(postResourceRequest)
    };
    var response = await GetHttpClient().SendAsync(request);
    response.EnsureSuccessStatusCode();
    var postResourceResponse = await response.Content.ReadFromJsonAsync<PostResourceResponse>();
    return postResourceResponse!;
  }

  protected async Task<PostClientResponse> BuildAuthorizationGrantWebClient(string name, string scope)
  {
    var postClientRequest = new PostClientRequest
    {
      ApplicationType = ApplicationTypeConstants.Web,
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Scope = scope,
      ClientName = name,
      GrantTypes = new[] { GrantTypeConstants.AuthorizationCode, GrantTypeConstants.RefreshToken },
      RedirectUris = new[] { "https://localhost:5002/callback" },
      SubjectType = SubjectTypeConstants.Public,
      ResponseTypes = new[] { ResponseTypeConstants.Code }
    };
    return await BuildClient(postClientRequest);
  }

  protected async Task<PostClientResponse> BuildAuthorizationGrantNativeClient(string name, string scope)
  {
    var postClientRequest = new PostClientRequest
    {
      ApplicationType = ApplicationTypeConstants.Native,
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.None,
      Scope = scope,
      ClientName = name,
      GrantTypes = new[] { GrantTypeConstants.AuthorizationCode, GrantTypeConstants.RefreshToken },
      RedirectUris = new[] { "https://localhost:5003/callback" },
      SubjectType = SubjectTypeConstants.Public,
      ResponseTypes = new[] { ResponseTypeConstants.Code }
    };
    return await BuildClient(postClientRequest);
  }

  protected async Task<PostClientResponse> BuildClientCredentialsWebClient(string name, string scope)
  {
    var postClientRequest = new PostClientRequest
    {
      ApplicationType = ApplicationTypeConstants.Web,
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Scope = scope,
      ClientName = name,
      GrantTypes = new[] { GrantTypeConstants.ClientCredentials },
      RedirectUris = new[] { "http://localhost:5002/callback" },
      SubjectType = SubjectTypeConstants.Public,
      ResponseTypes = new[] { ResponseTypeConstants.Code }
    };
    return await BuildClient(postClientRequest);
  }

  private async Task<PostClientResponse> BuildClient(PostClientRequest request)
  {
    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/client/register")
    {
      Content = JsonContent.Create(request)
    };
    var response = await GetHttpClient().SendAsync(requestMessage);
    response.EnsureSuccessStatusCode();
    var postClientResponse = await response.Content.ReadFromJsonAsync<PostClientResponse>();
    return postClientResponse!;
  }

  protected async Task<User> BuildUserAsync(string password)
  {
    var identityContext = _factory.Services.GetRequiredService<IdentityContext>();
    var user = UserBuilder
      .Instance()
      .AddPassword(password)
      .Build();
    await identityContext.Set<User>().AddAsync(user);
    await identityContext.SaveChangesAsync();
    return user;
  }
}