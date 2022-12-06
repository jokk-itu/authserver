using Contracts.PostToken;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Json;
using WebApp.Constants;
using Xunit;

namespace Specs.Integrations;

[Collection("Integration")]
public class ClientCredentialsTests : BaseIntegrationTest
{
  public ClientCredentialsTests(WebApplicationFactory<Program> factory) : base(factory)
  {
  }

  [Fact]
  [Trait("Category", "Integration")]
  public async Task ClientCredentials()
  {
    var client = await BuildClientCredentialsWebClient("test");
    var tokenContent = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      { ParameterNames.ClientId, client.ClientId },
      { ParameterNames.ClientSecret, client.ClientSecret },
      { ParameterNames.GrantType, OpenIdConnectGrantTypes.ClientCredentials },
      { ParameterNames.RedirectUri, "http://localhost:5002/callback" },
      { ParameterNames.Scope, "identityprovider:read" }
    });
    var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
    {
      Content = tokenContent
    };
    var tokenResponse = await Client.SendAsync(request);
    tokenResponse.EnsureSuccessStatusCode();
    var tokens = await tokenResponse.Content.ReadFromJsonAsync<PostTokenResponse>();
    Assert.NotNull(tokens);
    Assert.NotEmpty(tokens.AccessToken);
    Assert.True(tokens.ExpiresIn > 0);
  }
}
