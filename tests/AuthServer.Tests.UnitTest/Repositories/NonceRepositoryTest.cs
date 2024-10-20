using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Repositories;

public class NonceRepositoryTest : BaseUnitTest
{
    public NonceRepositoryTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task IsNonceReplay_NonceExists_ExpectTrue()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var nonceRepository = serviceProvider.GetRequiredService<INonceRepository>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        var value = CryptographyHelper.GetRandomString(32);
        var nonce = new Nonce(value, value.Sha256(), authorizationGrant);
        await AddEntity(nonce);

        // Act
        var isReplay = await nonceRepository.IsNonceReplay(value, CancellationToken.None);

        // Assert
        Assert.True(isReplay);
    }

    [Fact]
    public async Task IsNonceReplay_NonceDoesNotExist_ExpectFalse()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var nonceRepository = serviceProvider.GetRequiredService<INonceRepository>();

        var value = CryptographyHelper.GetRandomString(32);

        // Act
        var isReplay = await nonceRepository.IsNonceReplay(value, CancellationToken.None);

        // Assert
        Assert.False(isReplay);
    }
}
