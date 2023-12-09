using Microsoft.AspNetCore.Mvc.Testing;
using Domain.Constants;
using Infrastructure.Helpers;
using Specs.Helpers.EndpointBuilders;
using Xunit;
using Xunit.Abstractions;

namespace Specs.Integrations;

[Collection("Integration")]
public class ClientCredentialsTokenTest : BaseIntegrationTest
{
  public ClientCredentialsTokenTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    : base(factory, testOutputHelper)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ClientCredentials()
  {
    await CreateDatabase();
    await CreateIdentityProviderResource();
    const string scope = "weather:read";
    await BuildScope(scope);
    await BuildResource(scope, CryptographyHelper.GetRandomString(32), "weatherservice", "https://weather.authserver.dk");
    var client = await RegisterEndpointBuilder
      .Instance()
      .AddClientName("testapp")
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
    
    Assert.NotNull(tokens);
    Assert.NotEmpty(tokens.AccessToken);
    Assert.True(tokens.ExpiresIn > 0);
  }
}
