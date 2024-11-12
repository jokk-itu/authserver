using AuthServer.Authentication;
using AuthServer.Authentication.Abstractions;
using AuthServer.Authentication.Models;
using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.RequestAccessors.Authorize;
using AuthServer.Tests.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Authorize;

public class AuthorizeInteractionServiceTest : BaseUnitTest
{
    public AuthorizeInteractionServiceTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task GetInteractionResult_CallbackMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);
        await AddEntity(clientAuthenticationContextReference);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackConsentNotRequired_ExpectNone()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false,
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackConsentRequired_ExpectConsent()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.ConsentResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_CallbackFromConsent_ExpectNone()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(subjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        var authorizeUser = new AuthorizeUser(subjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
        authorizeUserAccessorMock.Verify();
    }

    [Theory]
    [InlineData(PromptConstants.Consent, ErrorCode.ConsentRequired)]
    [InlineData(PromptConstants.Login, ErrorCode.LoginRequired)]
    [InlineData(PromptConstants.SelectAccount, ErrorCode.AccountSelectionRequired)]
    public async Task GetInteractionResult_ClientProvidedPrompt_ExpectProvidedPrompt(string prompt, string errorCode)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                Prompt = prompt
            }, CancellationToken.None);

        // Assert
        Assert.False(interactionResult.IsSuccessful);
        Assert.Equal(errorCode, interactionResult.Error!.Error);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintExpiredGrant_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                IdTokenHint = idToken
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeUserAccessorMock);
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintWithInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintWithInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);
        await AddEntity(clientAuthenticationContextReference);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintConsentNotRequired_ExpectNone()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintConsentRequired_ExpectConsent()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.ConsentResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_IdTokenHintConsentRequired_ExpectNone()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(subjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, subjectIdentifier.Id, session.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
    }

    [Fact]
    public async Task GetInteractionResult_ZeroAuthenticatedUsers_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(0)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_MultipleAuthenticatedUsers_ExpectSelectAccount()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(2)
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.SelectAccountResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticationUserWithExpiredGrant_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.LoginResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserWithInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserWithInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);
        await AddEntity(clientAuthenticationContextReference);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.UnmetAuthenticationRequirementResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserConsentNotRequired_ExpectNone()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserConsentRequired_ExpectConsent()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(InteractionResult.ConsentResult, interactionResult);
        Assert.False(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetInteractionResult_OneAuthenticatedUserConsentRequired_ExpectNone()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(subjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(subjectIdentifier.Id))
            .Verifiable();

        // Act
        var interactionResult = await authorizeInteractionService.GetInteractionResult(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(subjectIdentifier.Id, interactionResult.SubjectIdentifier);
        Assert.True(interactionResult.IsSuccessful);
        authenticateUserAccessorMock.Verify();
    }
}