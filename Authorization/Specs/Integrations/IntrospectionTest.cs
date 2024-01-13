using Domain.Constants;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Xunit;
using Xunit.Abstractions;

namespace Specs.Integrations;

[Collection("Integration")]
public class IntrospectionTest : BaseIntegrationTest
{
  public IntrospectionTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : base(factory, testOutputHelper)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task Introspection_AccessToken()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    UseReferenceTokens();
    const string scope = "weather:read";
    await BuildScope(scope);

    var weatherClient = await RegisterEndpointBuilder
      .Instance()
      .AddClientName("weatherservice")
      .AddClientUri("https://weather.authserver.dk")
      .AddScope(scope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethodConstants.ClientSecretBasic)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .BuildClient(GetHttpClient());

    var client = await RegisterEndpointBuilder
      .Instance()
      .AddClientName("webapp")
      .AddScope(scope)
      .AddTokenEndpointAuthMethod(TokenEndpointAuthMethodConstants.ClientSecretPost)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .BuildClient(GetHttpClient());
    
    var tokens = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .AddScope(scope)
      .AddResource("https://weather.authserver.dk")
      .BuildRedeemClientCredentials(GetHttpClient());

    var introspection = await IntrospectionEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddToken(tokens.AccessToken)
      .AddTokenTypeHint(TokenTypeConstants.AccessToken)
      .BuildIntrospection(GetHttpClient());

    Assert.True(introspection.Active);
    Assert.Contains(weatherClient.ClientUri, introspection.Audience!);
    Assert.Equal(client.ClientId, introspection.ClientId);
    Assert.Equal(scope, introspection.Scope);
    Assert.Equal(TokenTypeConstants.AccessToken, introspection.TokenType);
  }
}