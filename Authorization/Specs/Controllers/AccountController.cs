﻿using Contracts.PostToken;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;
using Specs.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Web;
using Xunit;

namespace Specs.Controllers;
public class AccountController : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public AccountController(WebApplicationFactory<Program> applicationFactory)
	{
    _applicationFactory = applicationFactory;
	}

	[Fact]
	[Trait("Category", "Integration")]
	public async Task UserInfo_ExpectClaims()
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
      { "scope", "openid identity-provider profile api1 email phone" },
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

    // Act
    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "connect/v1/account/userinfo");
    userInfoRequest.Headers.Add("Authorization", $"Bearer {tokens!.AccessToken}");
    var userInfoResponse = await client.SendAsync(userInfoRequest);
    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
    var userInfoClaims = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(userInfoContent);

    // Assert
    Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);
    Assert.NotNull(userInfoClaims);
    Assert.NotEmpty(userInfoClaims);
    Assert.True(userInfoClaims!.TryGetValue(JwtRegisteredClaimNames.Sub, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimTypes.Name, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimTypes.Email, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimTypes.MobilePhone, out _));
  }
}