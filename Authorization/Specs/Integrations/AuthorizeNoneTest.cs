using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Specs.Helpers;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class AuthorizeNoneTest : BaseIntegrationTest
{
  public AuthorizeNoneTest(WebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ConfidentialClient_AuthorizeWithPromptNone()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId}";
    const string clientName = "webapp";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantWebClient(clientName, scope);
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddCodeChallenge(pkce.CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .BuildLoginAndConsent(GetHttpClient());

    var token = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .AddScope(scope)
      .AddCode(code)
      .AddCodeVerifier(pkce.CodeVerifier)
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var none = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddPrompt($"{PromptConstants.None}")
      .AddIdTokenHint(token.IdToken)
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .BuildNone(GetHttpClient());

    Assert.NotEmpty(code);
    Assert.NotNull(token);
    Assert.NotEmpty(none);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task NativeClient_AuthorizeWithPromptNone()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId}";
    const string clientName = "nativeapp";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantNativeClient(clientName, scope);
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddCodeChallenge(pkce.CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .BuildLoginAndConsent(GetHttpClient());

    var token = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .AddScope(scope)
      .AddCode(code)
      .AddCodeVerifier(pkce.CodeVerifier)
      .BuildRedeemAuthorizationCode(GetHttpClient());

    var none = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddPrompt($"{PromptConstants.None}")
      .AddIdTokenHint(token.IdToken)
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .BuildNone(GetHttpClient());

    Assert.NotEmpty(code);
    Assert.NotNull(token);
    Assert.NotEmpty(none);
  }
}
