using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.Token;
using AuthServer.TokenByGrant;
using AuthServer.TokenByGrant.ClientCredentialsGrant;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.ClientCredentialsGrant;
public class ClientCredentialsRequestProcessorTest : BaseUnitTest
{
    public ClientCredentialsRequestProcessorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_BuildAccessToken_ExpectTokenResponse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider
            .GetRequiredService<IRequestProcessor<ClientCredentialsValidatedRequest, TokenResponse>>();

        var client = new Client("worker-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            AccessTokenExpiration = 3600
        };
        var plainSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainSecret);
        client.SetSecret(hashedSecret);

        var clientCredentialsGrant = await GetGrantType(GrantTypeConstants.ClientCredentials);
        client.GrantTypes.Add(clientCredentialsGrant);

        var openIdScope = await GetScope(ScopeConstants.OpenId);
        client.Scopes.Add(openIdScope);

        await AddEntity(client);

        var weatherClient = new Client("weather-api", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            ClientUri = "https://weather.authserver.dk"
        };
        weatherClient.Scopes.Add(openIdScope);
        await AddEntity(weatherClient);

        var scope = new[] { ScopeConstants.OpenId };
        var resource = new[] { weatherClient.ClientUri };
        var request = new ClientCredentialsValidatedRequest
        {
            Scope = scope,
            Resource = resource,
            ClientId = client.Id
        };

        // Act
        var tokenResponse = await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.Equal(client.AccessTokenExpiration, tokenResponse.ExpiresIn);
        Assert.Equal(ScopeConstants.OpenId, tokenResponse.Scope);
        Assert.True(TokenHelper.IsJws(tokenResponse.AccessToken));
    }
}
