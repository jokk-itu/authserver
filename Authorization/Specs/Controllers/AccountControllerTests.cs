using Contracts.PostToken;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using Infrastructure.Helpers;
using Xunit;
using Domain.Constants;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Constants;

namespace Specs.Controllers;

[Collection("Integration")]
public class AccountControllerTests : BaseIntegrationTest
{

public AccountControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }

	[Fact]
	[Trait("Category", "Integration")]
	public async Task UserInfo_ExpectClaims()
	{
    // Arrange
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildClientAsync(ApplicationTypeConstants.Web, "test");
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile} {ScopeConstants.Phone} {ScopeConstants.Email}" },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce }
    }.ToQueryString();

    var authorizeResponse = await AuthorizeEndpointHelper.GetAuthorizationCodeAsync(Client, query, user.UserName, password);
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);
    var code = queryParameters.Get(ParameterNames.Code);
    Assert.NotEmpty(code);

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.Code, code},
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.AuthorizationCode },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone}" },
      { ParameterNames.CodeVerifier, pkce.CodeVerifier }
    });
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await Client.SendAsync(request);
    tokenResponse.EnsureSuccessStatusCode();
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Act
    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "connect/v1/account/userinfo");
    userInfoRequest.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {tokens!.AccessToken}");
    var userInfoResponse = await Client.SendAsync(userInfoRequest);
    userInfoResponse.EnsureSuccessStatusCode();
    var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
    var userInfoClaims = JsonSerializer.Deserialize<Dictionary<string, string>>(userInfoContent);

    // Assert
    Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);
    Assert.NotNull(userInfoClaims);
    Assert.NotEmpty(userInfoClaims);
    Assert.True(userInfoClaims!.TryGetValue(ClaimNameConstants.Sub, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimNameConstants.Name, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimNameConstants.Email, out _));
    Assert.True(userInfoClaims!.TryGetValue(ClaimNameConstants.Phone, out _));
  }
}
