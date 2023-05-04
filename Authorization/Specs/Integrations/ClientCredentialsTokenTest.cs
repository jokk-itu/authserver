using Microsoft.AspNetCore.Mvc.Testing;
using Domain.Constants;
using Specs.Helpers.EndpointBuilders;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class ClientCredentialsTokenTest : BaseIntegrationTest
{
  public ClientCredentialsTokenTest(WebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ClientCredentials()
  {
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
    
    Assert.NotNull(tokens);
    Assert.NotEmpty(tokens.AccessToken);
    Assert.True(tokens.ExpiresIn > 0);
  }
}
