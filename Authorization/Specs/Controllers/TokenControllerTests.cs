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

    // Act
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
    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/v1/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await client.SendAsync(tokenRequest);
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();

    // Act
    var refreshTokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, "test" },
      { ParameterNames.ClientSecret, "secret" },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.RefreshToken },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.RefreshToken, tokens!.RefreshToken }
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