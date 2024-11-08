using AuthServer.Authentication.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authentication;
public class ClientSectorServiceTest : BaseUnitTest
{
    public ClientSectorServiceTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task ContainsSectorDocument_ServerError_ExpectFalse()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var requestHandler = new DelegatingHandlerStub("Server error", MimeTypeConstants.Text, HttpStatusCode.InternalServerError);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });

        var clientSectorService = serviceProvider.GetRequiredService<IClientSectorService>();

        // Act
        var hasUris = await clientSectorService.ContainsSectorDocument(
            new Uri("https://client.authserver.dk/sector"),
            ["https://client.authserver.dk/jwks"],
            CancellationToken.None);

        // Arrange
        Assert.False(hasUris);
        httpClientFactory.Verify();
    }

    [Fact]
    public async Task ContainsSectorDocument_DocumentDoesNotContainUris_ExpectFalse()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var sectorDocument = new [] { "https://client.authserver.dk/callback" };
            var requestHandler = new DelegatingHandlerStub(JsonSerializer.Serialize(sectorDocument), MimeTypeConstants.Json, HttpStatusCode.OK);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });

        var clientSectorService = serviceProvider.GetRequiredService<IClientSectorService>();

        // Act
        var hasUris = await clientSectorService.ContainsSectorDocument(
            new Uri("https://client.authserver.dk/sector"),
            ["https://client.authserver.dk/jwks"],
            CancellationToken.None);

        // Arrange
        Assert.False(hasUris);
        httpClientFactory.Verify();
    }

    [Fact]
    public async Task ContainsSectorDocument_DocumentContainsUris_ExpectTrue()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var sectorDocument = new[] { "https://client.authserver.dk/jwks" };
            var requestHandler = new DelegatingHandlerStub(JsonSerializer.Serialize(sectorDocument), MimeTypeConstants.Json, HttpStatusCode.OK);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });

        var clientSectorService = serviceProvider.GetRequiredService<IClientSectorService>();

        // Act
        var hasUris = await clientSectorService.ContainsSectorDocument(
            new Uri("https://client.authserver.dk/sector"),
            ["https://client.authserver.dk/jwks"],
            CancellationToken.None);

        // Arrange
        Assert.True(hasUris);
        httpClientFactory.Verify();
    }
}