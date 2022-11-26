using Contracts.PostToken;
using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Specs.Helpers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Web;
using WebApp.Constants;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class AuthorizationRefreshTests : BaseIntegrationTest
{
  public AuthorizationRefreshTests(WebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task AuthorizationGrantWithConsentWithRefreshWithUserInfo()
  {
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildClient(ApplicationTypeConstants.Web, "test");
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      {
        ParameterNames.Scope,
        $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile} {ScopeConstants.Phone} {ScopeConstants.Email}"
      },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce },
      { ParameterNames.MaxAge, "120" },
      { ParameterNames.Prompt, "login consent" }
    };

    var loginResponse = await LoginEndpointHelper.GetLoginCode(Client, query.ToQueryString(), user.UserName, password, await GetAntiForgeryToken($"connect/login{query}"));
    var locationHeader = new Uri(Client.BaseAddress, loginResponse.Headers.Location);
    var loginCode = HttpUtility.ParseQueryString(locationHeader.Query).Get(ParameterNames.LoginCode);
    Assert.NotEmpty(loginCode);
    query.Add(ParameterNames.LoginCode, loginCode);
    var consentResponse = await ConsentEndpointHelper.GetConsent(Client, query.ToQueryString(), await GetAntiForgeryToken($"connect/consent{query}"));
    var code = HttpUtility.ParseQueryString(consentResponse.Headers.Location!.Query).Get(ParameterNames.Code);
    Assert.NotEmpty(code);

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.Code, code },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.AuthorizationCode },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile} {ScopeConstants.Phone} {ScopeConstants.Email}" },
      { ParameterNames.CodeVerifier, pkce.CodeVerifier }
    });
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await Client.SendAsync(request);
    tokenResponse.EnsureSuccessStatusCode();
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();
    Assert.NotNull(tokens);

    var refreshTokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.RefreshToken },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.RefreshToken, tokens!.RefreshToken }
    });
    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = refreshTokenContent
    };
    var refreshTokenResponse = await Client.SendAsync(refreshTokenRequest);
    refreshTokenResponse.EnsureSuccessStatusCode();
    var refreshedTokens = await refreshTokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "connect/userinfo");
    userInfoRequest.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {tokens!.AccessToken}");
    var userInfoResponse = await Client.SendAsync(userInfoRequest);
    userInfoResponse.EnsureSuccessStatusCode();
    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
    var userInfoClaims = JsonSerializer.Deserialize<Dictionary<string, string>>(userInfoContent);

    // Assert
    Assert.NotNull(refreshedTokens);
    Assert.NotEmpty(userInfoClaims);
  }
}