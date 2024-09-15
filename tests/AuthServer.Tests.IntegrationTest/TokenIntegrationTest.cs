using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Endpoints.Responses;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class TokenIntegrationTest : BaseIntegrationTest
{
    public TokenIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task Token_ClientCredentialsGrant_ExpectAccessToken()
    {
        // Arrange
        var scope = await AddWeatherReadScope();
        var weatherClient = await AddWeatherClient();

        var httpClient = GetHttpClient();
        var registerResponseMessage = await httpClient.PostAsJsonAsync(
            "connect/register",
            new Dictionary<string, object>
            {
                { Parameter.ClientName, "worker-app" },
                { Parameter.GrantTypes, new[] { GrantTypeConstants.ClientCredentials } },
                { Parameter.TokenEndpointAuthMethod, TokenEndpointAuthMethodConstants.ClientSecretPost },
                { Parameter.Scope, scope }
            });
        TestOutputHelper.WriteLine("Received Register response {0}, Content: {1}", registerResponseMessage.StatusCode, await registerResponseMessage.Content.ReadAsStringAsync());
        registerResponseMessage.EnsureSuccessStatusCode();
        var registerResponse = (await registerResponseMessage.Content.ReadFromJsonAsync<RegisterResponse>())!;

        var tokenBody = new List<KeyValuePair<string, string>>
        {
            new(Parameter.ClientId, registerResponse.ClientId),
            new(Parameter.ClientSecret, registerResponse.ClientSecret!),
            new(Parameter.GrantType, GrantTypeConstants.ClientCredentials),
            new(Parameter.Scope, scope),
            new(Parameter.Resource, weatherClient.ClientUri!)
        };
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "connect/token")
        {
            Content = new FormUrlEncodedContent(tokenBody)
        };

        // Act
        var tokenResponseMessage = await httpClient.SendAsync(tokenRequest);
        tokenResponseMessage.EnsureSuccessStatusCode();
        var tokenResponse = await tokenResponseMessage.Content.ReadFromJsonAsync<PostTokenResponse>();

        // Assert
        Assert.NotNull(tokenResponse);
        Assert.Equal(scope, tokenResponse.Scope);
        Assert.Equal("Bearer", tokenResponse.TokenType);
        Assert.Null(tokenResponse.RefreshToken);
        Assert.Null(tokenResponse.IdToken);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.Equal(registerResponse.AccessTokenExpiration, tokenResponse.ExpiresIn);
    }
}
