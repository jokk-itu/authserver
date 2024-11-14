using AuthServer.Tests.Core;
using Moq;
using System.Net;
using AuthServer.Authentication.Abstractions;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authentication;
public class ClientLogoutServiceTest : BaseUnitTest
{
    public ClientLogoutServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Logout_NoSessionOrSubjectClientReturnsInternalServerError_ExpectRequestSent()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var requestHandler = new DelegatingHandlerStub(HttpStatusCode.InternalServerError);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });

        var clientLogoutService = serviceProvider.GetRequiredService<IClientLogoutService>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            BackchannelLogoutUri = "https://webapp.authserver.dk/logout",
            IdTokenSignedResponseAlg = SigningAlg.RsaSha256
        };
        await AddEntity(client);

        // Act
        await clientLogoutService.Logout(client.Id, null, null, CancellationToken.None);

        // Assert
        httpClientFactory.Verify();
    }

    [Fact]
    public async Task Logout_SessionAndSubjectClientReturnsOk_ExpectRequestSent()
    {
        // Arrange
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            var requestHandler = new DelegatingHandlerStub(HttpStatusCode.OK);
            httpClientFactory
                .Setup(x => x.CreateClient(HttpClientNameConstants.Client))
                .Returns(new HttpClient(requestHandler))
                .Verifiable();

            services.AddSingletonMock(httpClientFactory);
        });

        var clientLogoutService = serviceProvider.GetRequiredService<IClientLogoutService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            BackchannelLogoutUri = "https://webapp.authserver.dk/logout",
            IdTokenSignedResponseAlg = SigningAlg.RsaSha256
        };
        await AddEntity(session);
        await AddEntity(client);

        // Act
        await clientLogoutService.Logout(client.Id, session.Id, subjectIdentifier.Id, CancellationToken.None);

        // Assert
        httpClientFactory.Verify();
    }
}
