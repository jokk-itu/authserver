using AuthServer.Constants;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Models;
using AuthServer.Tests.Core;
using AuthServer.TokenDecoders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit.ClientAuthenticationService;

public class ClientAssertionAuthenticationTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task AuthenticateClient_InvalidClientAssertionType_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var clientAuthentication =
            new ClientAssertionAuthentication(ClientTokenAudience.TokenEndpoint, null, "invalid_type", "");

        // Act
        var clientAuthenticationResult =
            await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_ClientDoesNotExist_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var clientAuthentication = new ClientAssertionAuthentication(ClientTokenAudience.TokenEndpoint,
            Guid.NewGuid().ToString(), ClientAssertionTypeConstants.PrivateKeyJwt, "");

        // Act
        var clientAuthenticationResult =
            await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_ClientIsNotRegisteredForPrivateKeyJwt_NotAuthenticated()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var client = await ClientBuilder.GetNativeClient();
        var clientAuthentication = new ClientAssertionAuthentication(ClientTokenAudience.TokenEndpoint, client.Id,
            ClientAssertionTypeConstants.PrivateKeyJwt, "");

        // Act
        var clientAuthenticationResult =
            await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateClient_TokenValidationFails_NotAuthenticated()
    {
        // Arrange
        var tokenDecoder = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            tokenDecoder
                .Setup(x => x.Validate(
                    It.IsAny<string>(), It.IsAny<ClientIssuedTokenDecodeArguments>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new SecurityTokenException())
                .Verifiable();

            services.AddScopedMock(tokenDecoder);
        });
        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();
        var clientJwks = ClientJwkBuilder.GetClientJwks();
        var client = await ClientBuilder.GetPrivateKeyJwtWebClient(clientJwks.PublicJwks);
        var clientAuthentication = new ClientAssertionAuthentication(ClientTokenAudience.TokenEndpoint, client.Id,
            ClientAssertionTypeConstants.PrivateKeyJwt, "");

        // Act
        var clientAuthenticationResult =
            await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Null(clientAuthenticationResult.ClientId);
        Assert.False(clientAuthenticationResult.IsAuthenticated);
        tokenDecoder.Verify();
    }

    [Fact]
    public async Task AuthenticateClient_ValidAssertion_Authenticated()
    {
        // Arrange
        var tokenDecoder = new Mock<ITokenDecoder<ClientIssuedTokenDecodeArguments>>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(tokenDecoder);
        });

        var clientJwks = ClientJwkBuilder.GetClientJwks();
        var client = await ClientBuilder.GetPrivateKeyJwtWebClient(clientJwks.PublicJwks);
        var token = ClientJwtBuilder.GetPrivateKeyJwt(client.Id, clientJwks.PrivateJwks, ClientTokenAudience.TokenEndpoint);
        var jsonWebToken = new JsonWebToken(token);
        tokenDecoder
            .Setup(x => x.Read(token))
            .ReturnsAsync(jsonWebToken)
            .Verifiable();

        tokenDecoder
            .Setup(x => x.Validate(token,
                It.Is<ClientIssuedTokenDecodeArguments>(a =>
                    a.ValidateLifetime && a.TokenTypes.Single() == TokenTypeHeaderConstants.PrivateKeyToken &&
                    a.ClientId == client.Id && a.Audience == ClientTokenAudience.TokenEndpoint),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonWebToken)
            .Verifiable();

        var clientAuthenticationService = serviceProvider.GetRequiredService<IClientAuthenticationService>();

        var clientAuthentication = new ClientAssertionAuthentication(ClientTokenAudience.TokenEndpoint, null,
            ClientAssertionTypeConstants.PrivateKeyJwt, token);

        // Act
        var clientAuthenticationResult =
            await clientAuthenticationService.AuthenticateClient(clientAuthentication, CancellationToken.None);

        // Assert
        Assert.Equal(client.Id, clientAuthenticationResult.ClientId);
        Assert.True(clientAuthenticationResult.IsAuthenticated);
        tokenDecoder.Verify();
    }
}