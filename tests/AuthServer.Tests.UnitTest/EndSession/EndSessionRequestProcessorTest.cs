using AuthServer.Authentication.Abstractions;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.EndSession;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.EndSession;
public class EndSessionRequestProcessorTest : BaseUnitTest
{
    public EndSessionRequestProcessorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Process_EmptyRequest_ExpectUnit()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var request = new EndSessionValidatedRequest();

        // Act
        var response = await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.IsType<Unit>(response);
    }

    [Fact]
    public async Task Process_LogoutAtIdentityProviderWithNoActiveGrants_ExpectRevokedSessionAndNoClientLogout()
    {
        // Arrange
        var clientLogoutService = new Mock<IClientLogoutService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(clientLogoutService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        var request = new EndSessionValidatedRequest
        {
            LogoutAtIdentityProvider = true,
            SessionId = session.Id
        };

        // Act
        await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.NotNull(session.RevokedAt);
        clientLogoutService.Verify(
            x => x.Logout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task Process_LogoutAtIdentityProviderWithActiveGrants_ExpectRevokedSessionAndOneClientLogout()
    {
        // Arrange
        var clientLogoutService = new Mock<IClientLogoutService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(clientLogoutService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            BackchannelLogoutUri = "https://webapp.authserver.dk/logout"
        };
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        await AddEntity(authorizationGrant);

        clientLogoutService
            .Setup(x => x.Logout(client.Id, session.Id, subjectIdentifier.Id, CancellationToken.None))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var request = new EndSessionValidatedRequest
        {
            LogoutAtIdentityProvider = true,
            SessionId = session.Id,
            SubjectIdentifier = subjectIdentifier.Id
        };

        // Act
        await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.NotNull(session.RevokedAt);
        clientLogoutService.Verify();
    }

    [Fact]
    public async Task Process_InactiveAuthorizationGrant_ExpectUnit()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            BackchannelLogoutUri = "https://webapp.authserver.dk/logout"
        };
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        var request = new EndSessionValidatedRequest
        {
            SessionId = session.Id,
            SubjectIdentifier = subjectIdentifier.Id,
            ClientId = client.Id
        };

        // Act
        var response = await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.IsType<Unit>(response);
    }

    [Fact]
    public async Task Process_ActiveAuthorizationGrant_ExpectRevokedGrantAndNoClientLogout()
    {
        // Arrange
        var clientLogoutService = new Mock<IClientLogoutService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(clientLogoutService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        await AddEntity(authorizationGrant);

        var request = new EndSessionValidatedRequest
        {
            SessionId = session.Id,
            SubjectIdentifier = subjectIdentifier.Id,
            ClientId = client.Id
        };

        // Act
        await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.NotNull(authorizationGrant.RevokedAt);
        clientLogoutService.Verify(
            x => x.Logout(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None),
            Times.Never);
    }

    [Fact]
    public async Task Process_ActiveAuthorizationGrant_ExpectRevokedGrantAndOneClientLogout()
    {
        // Arrange
        var clientLogoutService = new Mock<IClientLogoutService>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(clientLogoutService);
        });
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            BackchannelLogoutUri = "https://webapp.authserver.dk/logout"
        };
        var levelOfAssurance = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, levelOfAssurance);
        await AddEntity(authorizationGrant);

        clientLogoutService
            .Setup(x => x.Logout(client.Id, session.Id, subjectIdentifier.Id, CancellationToken.None))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var request = new EndSessionValidatedRequest
        {
            SessionId = session.Id,
            SubjectIdentifier = subjectIdentifier.Id,
            ClientId = client.Id
        };

        // Act
        await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.NotNull(authorizationGrant.RevokedAt);
        clientLogoutService.Verify();
    }

    [Fact]
    public async Task Process_NoClientId_ExpectUnit()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IRequestProcessor<EndSessionValidatedRequest, Unit>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        await AddEntity(session);

        var request = new EndSessionValidatedRequest
        {
            SessionId = session.Id,
            SubjectIdentifier = subjectIdentifier.Id
        };

        // Act
        var response = await processor.Process(request, CancellationToken.None);

        // Assert
        Assert.IsType<Unit>(response);
    }
}
