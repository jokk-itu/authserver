using AuthServer.Core.Abstractions;
using AuthServer.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit.ClientAuthenticationService;
public class ClientAuthenticationIdTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
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
        var client = await ClientBuilder.GetBasicWebClient();
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
        var client = await ClientBuilder.GetNativeClient();
        var clientAuthentication = new ClientIdAuthentication(client.Id);

        // Act
        var clientAuthenticationResult = await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Equal(client.Id, clientAuthenticationResult.ClientId);
        Assert.True(clientAuthenticationResult.IsAuthenticated);
    }
}