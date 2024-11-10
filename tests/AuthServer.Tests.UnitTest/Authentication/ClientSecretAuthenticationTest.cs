using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.Models;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authentication;

public class ClientSecretAuthenticationTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task AuthenticateClient_ClientDoesNotExist_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var clientId = Guid.NewGuid().ToString();
        var clientSecret = CryptographyHelper.GetRandomString(32);
        var clientAuthentication = new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, clientId, clientSecret);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_ClientIsNotRegisteredForNone_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = new Client("PinguApp", ApplicationType.Native, TokenEndpointAuthMethod.None);
        var clientSecret = CryptographyHelper.GetRandomString(32);
        await AddEntity(client);
        var clientAuthentication = new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, clientSecret);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_SecretExpired_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SecretExpiration = -1
        };
        var plainTextSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainTextSecret);
        client.SetSecret(hashedSecret);
        await AddEntity(client);
        var clientAuthentication = new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, plainTextSecret);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    public async Task AuthenticateClient_InvalidSecret_NotAuthenticated(string secret)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var plainTextSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainTextSecret);
        client.SetSecret(hashedSecret);
        await AddEntity(client);
        var clientAuthentication = new ClientSecretAuthentication(TokenEndpointAuthMethod.ClientSecretBasic, client.Id, secret);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Theory]
    [InlineData(TokenEndpointAuthMethod.ClientSecretBasic, 1)]
    [InlineData(TokenEndpointAuthMethod.ClientSecretPost, null)]
    public async Task AuthenticateClient_ValidSecret_Authenticated(TokenEndpointAuthMethod tokenEndpointAuthMethod, int? secretExpiration)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = new Client("PinguApp", ApplicationType.Web, tokenEndpointAuthMethod)
        {
            SecretExpiration = secretExpiration
        };
        var plainTextSecret = CryptographyHelper.GetRandomString(32);
        var hashedSecret = CryptographyHelper.HashPassword(plainTextSecret);
        client.SetSecret(hashedSecret);
        await AddEntity(client);
        var clientAuthentication = new ClientSecretAuthentication(tokenEndpointAuthMethod, client.Id, plainTextSecret);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Equal(client.Id, clientAuthenticationResult.ClientId);
        Assert.True(clientAuthenticationResult.IsAuthenticated);
    }
}