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
using System.Text.RegularExpressions;

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
    const string scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone} {IdentityProviderScope}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantClient(ApplicationTypeConstants.Web, "test", scope);
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, scope },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce },
      { ParameterNames.MaxAge, "120" },
      { ParameterNames.Prompt, $"{PromptConstants.Login} {PromptConstants.Consent}" }
    };

    var loginAntiForgery = await GetAntiForgeryToken($"connect/login{query}");
    var loginResponse = await LoginEndpointHelper.Login(Client, query.ToQueryString(), user.UserName, password, loginAntiForgery);
    var loginCookie = loginResponse.Headers.GetValues("Set-Cookie").Single();
    Assert.NotEmpty(loginCookie);

    var consentAntiForgery = await GetAntiForgeryToken($"connect/consent{query}", loginCookie);
    var consentResponse = await ConsentEndpointHelper.GetConsent(Client, query.ToQueryString(), consentAntiForgery, loginCookie);
    var html = await consentResponse.Content.ReadAsStringAsync();
    var authorizationCodeInput = Regex.Match(html, @"\<input name=""code"" type=""hidden"" value=""([^""]+)"" \/\>");
    Assert.Equal(2, authorizationCodeInput.Groups.Count);
    var code = authorizationCodeInput.Groups[1].Captures[0].Value;

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.Code, code },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.AuthorizationCode },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, scope },
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
    Assert.NotNull(userInfoClaims);
    Assert.NotEmpty(userInfoClaims);
  }
}