﻿using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Specs.Helpers;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class AuthorizeLoginTest : BaseIntegrationTest
{
  public AuthorizeLoginTest(WebApplicationFactory<Program> factory)
    : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task AuthorizeWithPromptLoginForConfidentialClient()
  {
    const string scope = $"{ScopeConstants.OpenId}";
    const string clientName = "webapp";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantWebClient(clientName, scope);
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
    const string scope = $"{ScopeConstants.OpenId}";
    const string clientName = "nativeapp";
    var password = CryptographyHelper.GetRandomString(32);
    var user = await BuildUserAsync(password);
    var client = await BuildAuthorizationGrantNativeClient(clientName, scope);
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
