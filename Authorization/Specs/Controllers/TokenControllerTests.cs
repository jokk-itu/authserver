using Contracts.PostToken;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Specs.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using Xunit;

namespace Specs.Controllers;
public class TokenControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public TokenControllerTests(WebApplicationFactory<Program> applicationFactory)
  {
    _applicationFactory = applicationFactory;
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostAuthorizationAsync_ExpectTokens()
  {
    // Arrange
    var client = _applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false
    });
    var (codeVerifier, codeChallenge) = ProofKeyForCodeExchangeHelper.GetCodes();
    var state = RandomGeneratorHelper.GeneratorRandomString(16);
    var nonce = RandomGeneratorHelper.GeneratorRandomString(32);
    var query = new QueryBuilder
    {
      { "response_type", "code" },
      { "client_id", "test" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "state", state },
      { "code_challenge", codeChallenge },
      { "code_challenge_method", "S256" },
      { "nonce", nonce }
    }.ToQueryString();

    // Act
    var loginViewResponse = await client.GetAsync($"connect/v1/authorize{query}");
    var (cookie, field) = await AntiForgeryHelper.GetAntiForgeryAsync(loginViewResponse);

    var postAuthorizeRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/v1/authorize{query}");
    postAuthorizeRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", cookie).ToString());
    var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "username", "jokk" },
      { "password", "Password12!" },
      { "AntiForgeryField", field }
    });
    postAuthorizeRequest.Content = loginForm;
    var authorizeResponse = await client.SendAsync(postAuthorizeRequest);
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "client_id", "test" },
      { "client_secret", "secret" },
      { "code", queryParameters.Get("code")! },
      { "grant_type", "authorization_code" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "code_verifier", codeVerifier }
    });
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await client.SendAsync(request);
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Assert
    Assert.NotNull(tokens);
    Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostRefreshAsync_ExpectTokens()
  {
    // Arrange
    var client = _applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
    {
      AllowAutoRedirect = false
    });
    var (codeVerifier, codeChallenge) = ProofKeyForCodeExchangeHelper.GetCodes();
    var state = RandomGeneratorHelper.GeneratorRandomString(16);
    var nonce = RandomGeneratorHelper.GeneratorRandomString(32);
    var query = new QueryBuilder
    {
      { "response_type", "code" },
      { "client_id", "test" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "state", state },
      { "code_challenge", codeChallenge },
      { "code_challenge_method", "S256" },
      { "nonce", nonce }
    }.ToQueryString();

    // Act
    var loginViewResponse = await client.GetAsync($"connect/v1/authorize{query}");
    var (cookie, field) = await AntiForgeryHelper.GetAntiForgeryAsync(loginViewResponse);

    var postAuthorizeRequest = new HttpRequestMessage(HttpMethod.Post, $"connect/v1/authorize{query}");
    postAuthorizeRequest.Headers.Add("Cookie", new CookieHeaderValue("AntiForgeryCookie", cookie).ToString());
    var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "username", "jokk" },
      { "password", "Password12!" },
      { "AntiForgeryField", field }
    });
    postAuthorizeRequest.Content = loginForm;
    var authorizeResponse = await client.SendAsync(postAuthorizeRequest);
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "client_id", "test" },
      { "client_secret", "secret" },
      { "code", queryParameters.Get("code")! },
      { "grant_type", "authorization_code" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "scope", "openid identity-provider profile api1" },
      { "code_verifier", codeVerifier }
    });
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await client.SendAsync(tokenRequest);
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    var refreshTokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { "client_id", "test" },
      { "client_secret", "secret" },
      { "grant_type", "refresh_token" },
      { "redirect_uri", "http://localhost:5002/callback" },
      { "refresh_token", tokens!.RefreshToken }
    });

    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = refreshTokenContent
    };

    var refreshTokenResponse = await client.SendAsync(refreshTokenRequest);
    var refreshedTokens = await refreshTokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();


    // Assert
    Assert.NotNull(refreshedTokens);
    Assert.Equal(HttpStatusCode.OK, refreshTokenResponse.StatusCode);
  }
}