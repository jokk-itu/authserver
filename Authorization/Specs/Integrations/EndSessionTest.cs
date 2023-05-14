using System.Net;
using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Specs.Helpers;
using Xunit;

namespace Specs.Integrations;
public class EndSessionTest : BaseIntegrationTest
{
  public EndSessionTest(WebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task EndSession_UsingGet()
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
      .AddScope(scope)
      .AddCodeVerifier(pkce.CodeVerifier)
      .AddCode(code)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var endSession = await EndSessionEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddIdTokenHint(tokenResponse.IdToken)
      .AddPostLogoutRedirectUri(client.PostLogoutRedirectUris.First())
      .AddState(CryptographyHelper.GetRandomString(16))
      .BuildEndSessionAsQuery(GetHttpClient());

    Assert.Equal(HttpStatusCode.Redirect, endSession);
  }


  [Fact]
  [Trait("Category", "Integration")]
  public async Task EndSession_UsingPost()
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
      .AddScope(scope)
      .AddCodeVerifier(pkce.CodeVerifier)
      .AddCode(code)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var endSession = await EndSessionEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddIdTokenHint(tokenResponse.IdToken)
      .AddPostLogoutRedirectUri(client.PostLogoutRedirectUris.First())
      .AddState(CryptographyHelper.GetRandomString(16))
      .BuildEndSessionAsPost(GetHttpClient());

    Assert.Equal(HttpStatusCode.Redirect, endSession);
  }
}