using Contracts.PostToken;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using System.Net;
using System.Net.Http.Json;
using System.Web;
using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebApp.Constants;
using Xunit;

namespace Specs.Controllers;
public class TokenControllerTests : BaseIntegrationTest
{
  public TokenControllerTests(WebApplicationFactory<Program> applicationFactory)
  : base(applicationFactory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task PostAuthorizationAsync_ExpectTokens()
  {
    // Arrange
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildClientAsync(ApplicationTypeConstants.Web, "test");
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
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
    var code = queryParameters.Get("code");
    Assert.NotEmpty(code);

    // Act
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
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await Client.SendAsync(request);
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
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildClientAsync(ApplicationTypeConstants.Web, "test");
    var state = CryptographyHelper.GetRandomString(16);
    var nonce = CryptographyHelper.GetRandomString(32);
    var pkce= ProofKeyForCodeExchangeHelper.GetPkce();
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
      { ParameterNames.Code, code },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.AuthorizationCode },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, $"{ScopeConstants.OpenId} identityprovider:read {ScopeConstants.Profile} {ScopeConstants.Phone} {ScopeConstants.Email}" },
      { ParameterNames.CodeVerifier, pkce.CodeVerifier }
    });
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await Client.SendAsync(tokenRequest);
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Act
    var refreshTokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.RefreshToken },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.RefreshToken, tokens!.RefreshToken }
    });
    var refreshTokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = refreshTokenContent
    };
    var refreshTokenResponse = await Client.SendAsync(refreshTokenRequest);
    var refreshedTokens = await refreshTokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Assert
    Assert.NotNull(refreshedTokens);
    Assert.Equal(HttpStatusCode.OK, refreshTokenResponse.StatusCode);
  }
}