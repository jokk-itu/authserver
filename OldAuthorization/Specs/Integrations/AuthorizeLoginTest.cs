using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Specs.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Specs.Integrations;

[Collection("Integration")]
public class AuthorizeLoginTest : BaseIntegrationTest
{
  public AuthorizeLoginTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : base(factory, testOutputHelper)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task AuthorizeWithPromptLoginForConfidentialClient()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await RegisterEndpointBuilder
      .Instance()
      .AddClientName("webapp")
      .AddScope(scope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethodConstants.ClientSecretPost)
      .AddRedirectUri("https://localhost:5002/callback")
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .BuildClient(GetHttpClient());

    var firstLoginWithConsent = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddMaxAge("0")
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .BuildLoginAndConsent(GetHttpClient());

    var secondLoginWithoutConsent = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddMaxAge("0")
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .AddPrompt($"{PromptConstants.Login}")
      .BuildLogin(GetHttpClient());

    Assert.NotEmpty(firstLoginWithConsent);
    Assert.NotEmpty(secondLoginWithoutConsent);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task AuthorizeWithPromptLoginForNativeClient()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = $"{ScopeConstants.OpenId}";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await RegisterEndpointBuilder
      .Instance()
      .AddClientName("webapp")
      .AddApplicationType(ApplicationTypeConstants.Native)
      .AddScope(scope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethodConstants.ClientSecretPost)
      .AddRedirectUri("https://localhost:5002/callback")
      .AddGrantType(GrantTypeConstants.AuthorizationCode)
      .BuildClient(GetHttpClient());

    var firstLoginWithConsent = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddMaxAge("0")
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .AddPrompt($"{PromptConstants.Login} {PromptConstants.Consent}")
      .BuildLoginAndConsent(GetHttpClient());

    var secondLoginWithoutConsent = await AuthorizeEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddScope(scope)
      .AddRedirectUri(client.RedirectUris.First())
      .AddUser(user.UserName, password)
      .AddMaxAge("0")
      .AddCodeChallenge(ProofKeyForCodeExchangeHelper.GetPkce().CodeChallenge)
      .AddPrompt($"{PromptConstants.Login}")
      .BuildLogin(GetHttpClient());

    Assert.NotEmpty(firstLoginWithConsent);
    Assert.NotEmpty(secondLoginWithoutConsent);
  }
}
