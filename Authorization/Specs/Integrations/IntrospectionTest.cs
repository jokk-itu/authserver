using Domain.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Specs.Helpers.EndpointBuilders;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class IntrospectionTest : BaseIntegrationTest
{
  public IntrospectionTest(WebApplicationFactory<Program> factory)
    : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task Introspection_AsResource()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    UseReferenceTokens();
    const string scope = "weather:read";
    await BuildScope(scope);
    var resource = await BuildResource(scope, "weatherservice");
    var client = await BuildClientCredentialsWebClient("test", scope);
    var tokens = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .AddScope(scope)
      .BuildRedeemClientCredentials(GetHttpClient());

    var introspection = await IntrospectionEndpointBuilder
      .Instance()
      .AddClientId(resource.Id)
      .AddClientSecret(resource.Secret)
      .AddToken(tokens.AccessToken)
      .AddTokenTypeHint(TokenTypeConstants.AccessToken)
      .BuildIntrospection(GetHttpClient());

    Assert.True(introspection.Active);
    Assert.Contains(resource.Name, introspection.Audience);
    Assert.Equal(client.ClientId, introspection.ClientId);
    Assert.Equal(scope, introspection.Scope);
    Assert.Equal(TokenTypeConstants.AccessToken, introspection.TokenType);
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task Introspection_AsClient()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    UseReferenceTokens();
    const string scope = "weather:read";
    await BuildScope(scope);
    var resource = await BuildResource(scope, "weatherservice");

    var client = await BuildClientCredentialsWebClient("test", scope);
    var tokens = await TokenEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddGrantType(GrantTypeConstants.ClientCredentials)
      .AddScope(scope)
      .BuildRedeemClientCredentials(GetHttpClient());

    var introspection = await IntrospectionEndpointBuilder
      .Instance()
      .AddClientId(client.ClientId)
      .AddClientSecret(client.ClientSecret)
      .AddToken(tokens.AccessToken)
      .AddTokenTypeHint(TokenTypeConstants.AccessToken)
      .BuildIntrospection(GetHttpClient());

    Assert.True(introspection.Active);
    Assert.Contains(resource.Name, introspection.Audience);
    Assert.Equal(client.ClientId, introspection.ClientId);
    Assert.Equal(scope, introspection.Scope);
    Assert.Equal(TokenTypeConstants.AccessToken, introspection.TokenType);
  }
}