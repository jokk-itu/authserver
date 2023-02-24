using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Specs.Helpers;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using WebApp.Constants;
using Xunit;
using System.Text.RegularExpressions;
using Specs.Helpers.EndpointBuilders;
using WebApp.Contracts.PostToken;

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
    var pkce = ProofKeyForCodeExchangeHelper.GetPkce();
    var code = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddCodeChallenge(pkce.CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .BuildLoginAndConsent(Client);

    var tokenResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddScope(scope)
      .AddCodeVerifier(pkce.CodeVerifier)
      .AddCode(code)
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .AddRedirectUri(client.RedirectUris.First())
      .BuildRedeemAuthorizationCode(Client);

    var refreshResponse = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.RefreshToken)
      .AddRefreshToken(tokenResponse.RefreshToken)
      .BuildRedeemRefreshToken(Client);

    var userInfo = await UserInfoEndpointBuilder
      .Instance()
      .AddAccessToken(refreshResponse.AccessToken)
      .BuildUserInfo(Client);

    Assert.NotEmpty(userInfo);
  }
}