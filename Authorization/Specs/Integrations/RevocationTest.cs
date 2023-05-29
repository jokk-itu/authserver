using System.Net;
using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers;
using Specs.Helpers.EndpointBuilders;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class RevocationTest : BaseIntegrationTest
{
  public RevocationTest(WebApplicationFactory<Program> factory)
    : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task Revocation_ReferenceToken()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    UseReferenceTokens();
    const string scope = "weather:read";
    await BuildScope(scope);
    await BuildResource(scope, "weatherservice");
    var client = await BuildClientCredentialsWebClient("test", scope);
    var tokens = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .AddScope(scope)
      .BuildRedeemClientCredentials(GetHttpClient());

    var revocation = await RevocationEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddToken(tokens.AccessToken)
      .AddTokenTypeHint(TokenTypeConstants.AccessToken)
      .BuildRevocation(GetHttpClient());

    Assert.Equal(HttpStatusCode.OK, revocation);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task Revocation_StructuredToken()
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

    var revocation = await RevocationEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddToken(tokenResponse.RefreshToken)
      .AddTokenTypeHint(TokenTypeConstants.RefreshToken)
      .BuildRevocation(GetHttpClient());

    Assert.Equal(HttpStatusCode.OK, revocation);
  }
}
