using AuthServer.Core.Abstractions;
using AuthServer.EndSession;
using AuthServer.EndSession.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.RequestAccessors.EndSession;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.EndSession;

public class EndSessionRequestValidatorTest : BaseUnitTest
{
    public EndSessionRequestValidatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task Validate_EmptyPostLogoutRedirectUriWithState_ExpectStateWithoutPostLogoutRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest
        {
            State = CryptographyHelper.GetRandomString(16)
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.StateWithoutPostLogoutRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_EmptyStateWithPostLogoutRedirectUri_ExpectPostLogoutRedirectUriWithoutState()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.PostLogoutRedirectUriWithoutState, processResult);
    }

    [Fact]
    public async Task Validate_EmptyIdTokenHintAndClientIdWithPostLogoutRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16)
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.PostLogoutRedirectUriWithoutClientIdOrIdTokenHint, processResult);
    }

    [Fact]
    public async Task Validate_InvalidJwtIdTokenHint_ExpectInvalidIdToken()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            IdTokenHint = "invalid_id_token_hint"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.InvalidIdToken, processResult);
    }

    [Fact]
    public async Task Validate_MismatchingClientIdFromIdTokenHintAndClientId_ExpectMismatchingClientId()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var idToken = JwtBuilder.GetIdToken(
            "client_id",
            "grant_id",
            "subject",
            "sessionId",
            [],
            LevelOfAssuranceLow);

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            IdTokenHint = idToken,
            ClientId = "invalid_client_id"
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.InvalidIdToken, processResult);
    }

    [Fact]
    public async Task Validate_IdTokenHintClientIsNotRegisteredWithPostLogoutRedirectUri_ExpectUnauthorizedClientForPostLogoutRedirectUri()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var idToken = JwtBuilder.GetIdToken(
            client.Id,
            "grant_id",
            "subject",
            "sessionId",
            [],
            LevelOfAssuranceLow);

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            IdTokenHint = idToken,
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal(EndSessionError.UnauthorizedClientForPostLogoutRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_ValidIdTokenHintWithPostLogoutRedirectUri_ExpectEndSessionValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var postLogoutRedirectUri = new PostLogoutRedirectUri("https://webapp.authserver.dk", client);
        await AddEntity(postLogoutRedirectUri);

        var idToken = JwtBuilder.GetIdToken(
            client.Id,
            "grant_id",
            "subject",
            "sessionId",
            [],
            LevelOfAssuranceLow);

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            IdTokenHint = idToken,
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal("subject", processResult.Value!.SubjectIdentifier);
        Assert.Equal("sessionId", processResult.Value!.SessionId);
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.True(processResult.Value!.LogoutAtIdentityProvider);
    }

    [Fact]
    public async Task Validate_ValidIdTokenHintWithoutPostLogoutRedirectUri_ExpectEndSessionValidatedRequest()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var idToken = JwtBuilder.GetIdToken(
            client.Id,
            "grant_id",
            "subject",
            "sessionId",
            [],
            LevelOfAssuranceLow);

        var request = new EndSessionRequest
        {
            IdTokenHint = idToken
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Assert
        Assert.Equal("subject", processResult.Value!.SubjectIdentifier);
        Assert.Equal("sessionId", processResult.Value!.SessionId);
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.True(processResult.Value!.LogoutAtIdentityProvider);
    }

    [Fact]
    public async Task Validate_EmptyEndSessionUser_ExpectInteractionRequired()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IEndSessionUserAccessor>());
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Arrange
        Assert.Equal(EndSessionError.InteractionRequired, processResult);
    }

    [Fact]
    public async Task Validate_InactiveSessionWithoutClientId_ExpectEndSessionValidatedRequest()
    {
        // Arrange
        var endSessionUserAccessor = new Mock<IEndSessionUserAccessor>();
        var endSessionUser = new EndSessionUser("subject", true);
        var serviceProvider = BuildServiceProvider(services =>
        {
            endSessionUserAccessor
                .Setup(x => x.TryGetUser())
                .Returns(endSessionUser)
                .Verifiable();

            services.AddScopedMock(endSessionUserAccessor);
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var request = new EndSessionRequest();

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Arrange
        Assert.Equal(endSessionUser.SubjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Null(processResult.Value!.SessionId);
        Assert.Null(processResult.Value!.ClientId);
        Assert.Equal(endSessionUser.LogoutAtIdentityProvider, processResult.Value!.LogoutAtIdentityProvider);
    }

    [Fact]
    public async Task Validate_ClientIdClientIsNotRegisteredWithPostLogoutRedirectUri_ExpectUnauthorizedClientForPostLogoutRedirectUri()
    {
        // Arrange
        var endSessionUserAccessor = new Mock<IEndSessionUserAccessor>();
        var endSessionUser = new EndSessionUser("subject", true);
        var serviceProvider = BuildServiceProvider(services =>
        {
            endSessionUserAccessor
                .Setup(x => x.TryGetUser())
                .Returns(endSessionUser)
                .Verifiable();

            services.AddScopedMock(endSessionUserAccessor);
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Arrange
        Assert.Equal(EndSessionError.UnauthorizedClientForPostLogoutRedirectUri, processResult);
    }

    [Fact]
    public async Task Validate_ActiveSessionWithPostLogoutRedirectUri_ExpectEndSessionValidatedRequest()
    {
        // Arrange
        var endSessionUserAccessor = new Mock<IEndSessionUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(endSessionUserAccessor);
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        await AddEntity(session);

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var postLogoutRedirectUri = new PostLogoutRedirectUri("https://webapp.authserver.dk", client);
        await AddEntity(postLogoutRedirectUri);

        var endSessionUser = new EndSessionUser(subjectIdentifier.Id, true);
        endSessionUserAccessor
            .Setup(x => x.TryGetUser())
            .Returns(endSessionUser)
            .Verifiable();

        var request = new EndSessionRequest
        {
            PostLogoutRedirectUri = "https://webapp.authserver.dk",
            State = CryptographyHelper.GetRandomString(16),
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Arrange
        Assert.Equal(endSessionUser.SubjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(session.Id, processResult.Value!.SessionId);
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.Equal(endSessionUser.LogoutAtIdentityProvider, processResult.Value!.LogoutAtIdentityProvider);
    }

    [Fact]
    public async Task Validate_ActiveSessionWithoutPostLogoutRedirectUri_ExpectEndSessionValidatedRequest()
    {
        // Arrange
        var endSessionUserAccessor = new Mock<IEndSessionUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(endSessionUserAccessor);
        });
        var validator = serviceProvider
            .GetRequiredService<IRequestValidator<EndSessionRequest, EndSessionValidatedRequest>>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        await AddEntity(session);

        var client = new Client("web-app", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await AddEntity(client);

        var endSessionUser = new EndSessionUser(subjectIdentifier.Id, true);
        endSessionUserAccessor
            .Setup(x => x.TryGetUser())
            .Returns(endSessionUser)
            .Verifiable();

        var request = new EndSessionRequest
        {
            ClientId = client.Id
        };

        // Act
        var processResult = await validator.Validate(request, CancellationToken.None);

        // Arrange
        Assert.Equal(endSessionUser.SubjectIdentifier, processResult.Value!.SubjectIdentifier);
        Assert.Equal(session.Id, processResult.Value!.SessionId);
        Assert.Equal(client.Id, processResult.Value!.ClientId);
        Assert.Equal(endSessionUser.LogoutAtIdentityProvider, processResult.Value!.LogoutAtIdentityProvider);
    }
}
