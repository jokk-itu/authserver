using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using Xunit;
using Specs.Helpers.EndpointBuilders;
using Xunit.Abstractions;

namespace Specs.Integrations;

[Collection("Integration")]
public class AuthorizationCodeTokenTest : BaseIntegrationTest
{
  public AuthorizationCodeTokenTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : base(factory, testOutputHelper)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ConfidentialClient_AuthorizationGrant()
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
      .AddResource("https://idp.authserver.dk")
      .BuildRedeemAuthorizationCode(GetHttpClient());

    Assert.NotNull(tokenResponse);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task NativeClient_AuthorizationGrant()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId} {ScopeConstants.Profile} {ScopeConstants.Email} {ScopeConstants.Phone} {ScopeConstants.UserInfo}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantNativeClient("webapp", scope);
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
      .AddResource("https://idp.authserver.dk")
      .BuildRedeemAuthorizationCode(GetHttpClient());

    Assert.NotNull(tokenResponse);
  }
}