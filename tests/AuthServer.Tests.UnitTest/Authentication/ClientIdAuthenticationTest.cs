using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.Models;
using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authentication;
public class ClientIdAuthenticationTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task AuthenticateClient_ClientDoesNotExist_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var clientAuthentication = new ClientIdAuthentication(Guid.NewGuid().ToString());

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
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);
        var clientAuthentication = new ClientIdAuthentication(client.Id);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_ClientIsRegisteredForNone_Authenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = new Client("PinguNativeApp", ApplicationType.Native, TokenEndpointAuthMethod.None);
        await AddEntity(client);
        var clientAuthentication = new ClientIdAuthentication(client.Id);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Equal(client.Id, clientAuthenticationResult.ClientId);
        Assert.True(clientAuthenticationResult.IsAuthenticated);
    }
}