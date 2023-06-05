using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using Xunit;
using Specs.Helpers.EndpointBuilders;
using Xunit.Abstractions;

namespace Specs.Integrations;

[Collection("Integration")]
public class RefreshTestTokenTest : BaseIntegrationTest
{
  public RefreshTestTokenTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : base(factory, testOutputHelper)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ConfidentialClient_RefreshToken()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone} {ScopeConstants.UserInfo}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantWebClient("webapp", scope);
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddCodeChallenge(pkce.CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .BuildLoginAndConsent(GetHttpClient());

    var tokenResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddCodeVerifier(pkce.CodeVerifier)
      .AddCode(code)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var refreshResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.RefreshToken)
      .AddScope(scope)
      .AddRefreshToken(tokenResponse.RefreshToken)
      .BuildRedeemRefreshToken(GetHttpClient());

    Assert.NotNull(refreshResponse);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task NativeClient_RefreshToken()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone} {ScopeConstants.UserInfo}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantNativeClient("nativeapp", scope);
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddCodeChallenge(pkce.CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .BuildLoginAndConsent(GetHttpClient());

    var tokenResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddCodeVerifier(pkce.CodeVerifier)
      .AddCode(code)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var refreshResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddGrantType(GrantTypeConstants.RefreshToken)
      .AddScope(scope)
      .AddRefreshToken(tokenResponse.RefreshToken)
      .BuildRedeemRefreshToken(GetHttpClient());

    Assert.NotNull(refreshResponse);
  }
}