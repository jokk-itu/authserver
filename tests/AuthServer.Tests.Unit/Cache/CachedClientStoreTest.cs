using System.Reflection;
using AuthServer.Cache;
using AuthServer.Cache.Abstractions;
using AuthServer.Cache.Entities;
using AuthServer.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit.Cache;

public class CachedClientStoreTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    // Get_ClientNotFoundException

    [Fact]
    public async Task TryGet_InternalCacheHit_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(new Mock<IDistributedCache>());
        });

        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

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
    public async Task TryGet_DistributedCacheHit_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

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
    public async Task TryGet_DistributedCacheMiss_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

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
    public async Task Get_InternalCacheHit_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingletonMock(new Mock<IDistributedCache>());
        });

        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

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
    public async Task Get_DistributedCacheHit_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

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
        var cachedClient = await cachedClientStore.Get(client.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedClient);
        Assert.Equal(client.Id, cachedClient.Id);
        distributedCacheMock.Verify();
    }

    [Fact]
    public async Task Get_DistributedCacheMiss_CachedClientNotNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();
        var client = await ClientBuilder.GetBasicWebClient();

        // Act
        var cachedClient = await cachedClientStore.Get(client.Id, CancellationToken.None);

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
    public async Task Get_DistributedCacheMiss_CachedClientNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = BuildServiceProvider(services => { services.AddSingletonMock(distributedCacheMock); });
        var cachedClientStore = serviceProvider.GetRequiredService<ICachedClientStore>();

        // Act & Assert
        await Assert.ThrowsAsync<ClientNotFoundException>(() => cachedClientStore.Get(Guid.NewGuid().ToString(), CancellationToken.None));

        distributedCacheMock.Verify(
            distributedCache => distributedCache.Get<CachedClient>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once());
        distributedCacheMock.Verify(
            distributedCache => distributedCache.Add(It.IsAny<string>(), It.IsAny<CachedClient>(), It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }
}