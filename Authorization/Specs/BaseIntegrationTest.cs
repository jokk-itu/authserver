using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Specs.Helpers;
using Specs.Helpers.Builders;
using WebApp.Constants;
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
  protected const string IdentityProviderScope = "identityprovider:read";

  public HttpClient Client { get; set; }
  private string? Cookie { get; set; }

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

      BuildScope(IdentityProviderScope).GetAwaiter().GetResult();
      BuildResource(IdentityProviderScope, "identityprovider").GetAwaiter().GetResult();
  }

  protected async Task<PostScopeResponse> BuildScope(string scope)
  {
    var getInitialToken = await Client.GetFromJsonAsync<GetScopeInitialAccessToken>("connect/scope/initial-token");
    if (getInitialToken is null)
    {
      throw new Exception("scope initial-token failed");
    }

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
    {
      throw new Exception();
    }

    return postScopeResponse;
  }

  protected async Task<PostResourceResponse> BuildResource(string scope, string name)
  {
    var getInitialToken = await Client.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/resource/initial-token");
    if (getInitialToken is null)
    {
      throw new Exception("resource initial-token failed");
    }

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
    {
      throw new Exception();
    }

    return postResourceResponse;
  }

  protected async Task<PostClientResponse> BuildAuthorizationGrantClient(string applicationType, string name, string scope)
  {
    var postClientRequest = new PostClientRequest
    {
      ApplicationType = applicationType,
      TokenEndpointAuthMethod = TokenEndpointAuthMethodConstants.ClientSecretPost,
      Scope = scope,
      ClientName = name,
      GrantTypes = new[] { GrantTypeConstants.AuthorizationCode, GrantTypeConstants.RefreshToken },
      RedirectUris = new[] { "http://localhost:5002/callback" },
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
    var getInitialToken = await Client.GetFromJsonAsync<GetClientInitialAccessTokenResponse>("connect/client/initial-token");
    if (getInitialToken is null)
    {
      throw new Exception("client initial-token failed");
    }

    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "connect/client/register")
    {
      Content = JsonContent.Create(request)
    };
    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", getInitialToken.AccessToken);
    var response = await Client.SendAsync(requestMessage);
    response.EnsureSuccessStatusCode();
    var postClientResponse = await response.Content.ReadFromJsonAsync<PostClientResponse>();
    if (postClientResponse is null)
    {
      throw new JsonException();
    }

    return postClientResponse;
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

  protected async Task<AntiForgeryToken> GetAntiForgeryToken(string path)
  {
    var response = await Client.GetAsync(path);
    var html = await response.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(Cookie))
    {
      var antiForgeryCookie = response.Headers
        .GetValues("Set-Cookie")
        .FirstOrDefault(x => x.Contains(AntiForgeryConstants.AntiForgeryCookie));

      var antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value;
      if (string.IsNullOrWhiteSpace(antiForgeryCookieValue.Value))
      {
        throw new Exception("Invalid cookie was provided");
      }

      Cookie = antiForgeryCookieValue.Value;
    }

    var antiForgeryFieldMatch = Regex.Match(html, $@"\<input name=""{AntiForgeryConstants.AntiForgeryField}"" type=""hidden"" value=""([^""]+)"" \/\>");
    if (!antiForgeryFieldMatch.Captures.Any() && antiForgeryFieldMatch.Groups.Count != 2)
    {
      throw new Exception("Invalid input of anti-forgery-token was provided");
    }

    var antiForgeryField = antiForgeryFieldMatch.Groups[1].Captures[0].Value;

    return new AntiForgeryToken
    {
      Cookie = Cookie,
      Field = antiForgeryField
    };
  }
}
