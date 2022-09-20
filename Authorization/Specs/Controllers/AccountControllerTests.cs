using Contracts.PostToken;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Specs.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Web;
using Infrastructure.Helpers;
using Xunit;
using Domain.Constants;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Constants;

namespace Specs.Controllers;
public class AccountControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
  private readonly WebApplicationFactory<Program> _applicationFactory;

  public AccountControllerTests(WebApplicationFactory<Program> applicationFactory)
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
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
    var query = new QueryBuilder
    {
      { ParameterNames.ResponseType, ResponseTypeConstants.Code },
      { ParameterNames.ClientId, "test" },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identity-provider {ScopeConstants.Profile} api1" },
      { ParameterNames.State, state },
      { ParameterNames.CodeChallenge, pkce.CodeChallenge },
      { ParameterNames.CodeChallengeMethod, CodeChallengeMethodConstants.S256 },
      { ParameterNames.Nonce, nonce }
    }.ToQueryString();

    var authorizeResponse = await AuthorizeEndpointHelper.GetAuthorizationCodeAsync(client, query, "jokk", "Password12!");
    var queryParameters = HttpUtility.ParseQueryString(authorizeResponse.Headers.Location!.Query);

    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, "test" },
      { ParameterNames.ClientSecret, "secret" },
      { ParameterNames.Code, queryParameters.Get("code")! },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.AuthorizationCode },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identity-provider {ScopeConstants.Profile} api1" },
      { ParameterNames.CodeVerifier, pkce.CodeVerifier }
    });
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await client.SendAsync(request);
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Act
    var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "connect/v1/account/userinfo");
    userInfoRequest.Headers.Add(HttpRequestHeader.Authorization.ToString(), $"Bearer {tokens!.AccessToken}");
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
