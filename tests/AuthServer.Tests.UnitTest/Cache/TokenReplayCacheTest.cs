using AuthServer.Cache.Abstractions;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Cache;

public class TokenReplayCacheTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public void TryAdd_AddTokenSuccess_TokenIsInCache()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(distributedCacheMock);
        });
        var tokenReplayCache = serviceProvider.GetRequiredService<ITokenReplayCache>();

        // Act
        var securityToken = "security-token";
        var expiresOn = DateTime.UtcNow.AddSeconds(30);
        var isStored = tokenReplayCache.TryAdd(securityToken, expiresOn);

        // Assert
        Assert.True(isStored);
        distributedCacheMock.Verify(distributedCache => distributedCache.Add(It.IsAny<string>(), securityToken, expiresOn, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public void TryFind_FindTokenSuccess_TokenIsInCache()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(distributedCacheMock);
        });
        var tokenReplayCache = serviceProvider.GetRequiredService<ITokenReplayCache>();

        // Act
        var securityToken = "security-token";
        distributedCacheMock
            .Setup(x => x.Get<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(securityToken)
            .Verifiable();

        var isFetched = tokenReplayCache.TryFind(securityToken);

        // Assert
        Assert.True(isFetched);
        distributedCacheMock.Verify();
    }

    [Fact]
    public void TryFind_TokenDoesNotExist_TokenIsNotInCache()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(distributedCacheMock);
        });
        var tokenReplayCache = serviceProvider.GetRequiredService<ITokenReplayCache>();

        // Act
        var securityToken = "security-token";
        distributedCacheMock
            .Setup(x => x.Get<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null)
            .Verifiable();

        var isFetched = tokenReplayCache.TryFind(securityToken);

        // Assert
        Assert.False(isFetched);
        distributedCacheMock.Verify();
    }
}