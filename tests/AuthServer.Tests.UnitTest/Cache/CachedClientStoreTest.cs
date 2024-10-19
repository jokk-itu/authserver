using System.Reflection;
using AuthServer.Cache;
using AuthServer.Cache.Abstractions;
using AuthServer.Cache.Entities;
using AuthServer.Cache.Exceptions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Cache;

public class CachedClientStoreTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task TryGet_InternalCacheHit_ExpectCachedClient()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(new Mock<IDistributedCache>());
        });

        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        await ((CachedClientStore)cachedClientStore).Add(client.Id, CancellationToken.None);

        // Act
        var cachedClient = await cachedClientStore.TryGet(client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedClient);
        Assert.Equal(client.Id, cachedClient.Id);
        distributedCacheMock.Verify(
            distributedCache => distributedCache.Get<CachedClient>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryGet_DistributedCacheHit_ExpectCachedClient()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var tempCachedClient = await ((CachedClientStore)cachedClientStore).Add(client.Id, CancellationToken.None);
        distributedCacheMock
            .Setup(x => x.Get<CachedClient>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tempCachedClient)
            .Verifiable();

        cachedClientStore
            .GetType()
            .GetField("_internalCache", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(cachedClientStore, new Dictionary<string, CachedClient>());

        // Act
        var cachedClient = await cachedClientStore.TryGet(client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedClient);
        Assert.Equal(client.Id, cachedClient.Id);
        distributedCacheMock.Verify();
    }

    [Fact]
    public async Task TryGet_DistributedCacheMiss_ExpectCachedClient()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        // Act
        var cachedClient = await cachedClientStore.TryGet(client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedClient);
        Assert.Equal(client.Id, cachedClient.Id);
        distributedCacheMock.Verify(
            distributedCache => distributedCache.Get<CachedClient>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once());
        distributedCacheMock.Verify(
            distributedCache => distributedCache.Add(It.IsAny<string>(), It.IsAny<CachedClient>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task TryGet_DatabaseMiss_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();

        // Act
        var cachedClient = await cachedClientStore.TryGet("invalid_client_id", CancellationToken.None);

        // Assert
        Assert.Null(cachedClient);
    }

    [Fact]
    public async Task Get_InternalCacheHit_ExpectCachedClient()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(new Mock<IDistributedCache>());
        });

        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        await ((CachedClientStore)cachedClientStore).Add(client.Id, CancellationToken.None);

        // Act
        var cachedClient = await cachedClientStore.Get(client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedClient);
        Assert.Equal(client.Id, cachedClient.Id);
        distributedCacheMock.Verify(
            distributedCache => distributedCache.Get<CachedClient>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Get_InternalCacheMiss_ExpectClientNotFoundException()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();

        // Act & Assert
        await Assert.ThrowsAsync<ClientNotFoundException>(() => cachedClientStore.Get("invalid_client_id", CancellationToken.None));
    }
}