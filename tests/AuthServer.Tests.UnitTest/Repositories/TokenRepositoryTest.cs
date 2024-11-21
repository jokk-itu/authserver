using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Repositories;
public class TokenRepositoryTest : BaseUnitTest
{
    public TokenRepositoryTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task GetRegistrationToken_ActiveToken_ExpectRegistrationToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new RegistrationToken(client, "webapp", DiscoveryDocument.Issuer, ScopeConstants.Register);
        await AddEntity(token);

        var tokenRepository = serviceProvider.GetRequiredService<ITokenRepository>();

        // Act
        var registrationToken = await tokenRepository.GetActiveRegistrationToken(token.Reference, CancellationToken.None);

        // Assert
        Assert.NotNull(registrationToken);
        Assert.Equal(token, registrationToken);
        Assert.Equal(client, registrationToken.Client);
    }

    [Fact]
    public async Task GetRegistrationToken_InvalidReference_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var tokenRepository = serviceProvider.GetRequiredService<ITokenRepository>();

        // Act
        var token = await tokenRepository.GetActiveRegistrationToken("invalid_reference", CancellationToken.None);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public async Task GetRegistrationToken_RevokedToken_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var token = new RegistrationToken(client, "webapp", DiscoveryDocument.Issuer, ScopeConstants.Register);
        token.Revoke();
        await AddEntity(token);

        var tokenRepository = serviceProvider.GetRequiredService<ITokenRepository>();

        // Act
        var registrationToken = await tokenRepository.GetActiveRegistrationToken(token.Reference, CancellationToken.None);

        // Assert
        Assert.Null(registrationToken);
    }
}
