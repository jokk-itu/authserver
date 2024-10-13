using AuthServer.Authorize;
using AuthServer.Authorize.Abstractions;
using AuthServer.Constants;
using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.RequestAccessors.Authorize;
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
    public async Task GetPrompt_CallbackMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);
        await AddEntity(clientAuthenticationContextReference);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackConsentNotRequired_ExpectNone()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false,
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId],
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackConsentRequired_ExpectConsent()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Consent, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_CallbackFromConsent_ExpectNone()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.TryGetUser())
            .Returns(authorizeUser)
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Theory]
    [InlineData(PromptConstants.Consent)]
    [InlineData(PromptConstants.Login)]
    [InlineData(PromptConstants.SelectAccount)]
    public async Task GetPrompt_ClientProvidedPrompt_ExpectProvidedPrompt(string prompt)
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                Prompt = prompt
            }, CancellationToken.None);

        // Assert
        Assert.Equal(prompt, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintExpiredGrant_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                IdTokenHint = idToken
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeUserAccessorMock);
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services => { services.AddScopedMock(authorizeUserAccessorMock); });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        typeof(AuthorizationGrant)
            .GetProperty(nameof(AuthorizationGrant.AuthTime))!
            .SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-180));

        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintWithInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintWithInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
        {
            AuthenticationMethodReferences =
                [await GetAuthenticationMethodReference(AuthenticationMethodReferenceConstants.Password)]
        };
        await AddEntity(authorizationGrant);
        await AddEntity(clientAuthenticationContextReference);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintConsentNotRequired_ExpectNone()
    {
        // Arrange
        var authorizeUserAccessorMock = new Mock<IAuthorizeUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authorizeUserAccessorMock);
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        var authorizeUser = new AuthorizeUser(publicSubjectIdentifier.Id);
        authorizeUserAccessorMock
            .Setup(x => x.SetUser(authorizeUser))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
        authorizeUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintConsentRequired_ExpectConsent()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Consent, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_IdTokenHintConsentRequired_ExpectNone()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        var idToken = JwtBuilder.GetIdToken(
            client.Id, authorizationGrant.Id, publicSubjectIdentifier.Id,
            [AuthenticationMethodReferenceConstants.Password], LevelOfAssuranceLow);

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                IdTokenHint = idToken,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
    }

    [Fact]
    public async Task GetPrompt_ZeroAuthenticatedUsers_ExpectLogin()
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
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_MultipleAuthenticatedUsers_ExpectSelectAccount()
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
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest(), CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.SelectAccount, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticationUserWithExpiredGrant_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
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
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                MaxAge = "30"
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserDefaultMaxAgeExceeded_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            DefaultMaxAge = 30
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
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
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserWithInsufficientAuthenticationMethodReferenceAgainstRequest_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
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
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                AcrValues = [LevelOfAssuranceSubstantial]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserWithInsufficientAuthenticationMethodReferenceAgainstDefault_ExpectLogin()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var substantialAcr = await GetAuthenticationContextReference(LevelOfAssuranceSubstantial);
        var clientAuthenticationContextReference = new ClientAuthenticationContextReference(client, substantialAcr, 0);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr)
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
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Login, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserConsentNotRequired_ExpectNone()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserConsentRequired_ExpectConsent()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.Consent, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }

    [Fact]
    public async Task GetPrompt_OneAuthenticatedUserConsentRequired_ExpectNone()
    {
        // Arrange
        var authenticateUserAccessorMock = new Mock<IAuthenticatedUserAccessor>();
        var serviceProvider = BuildServiceProvider(services =>
        {
            services.AddScopedMock(authenticateUserAccessorMock);
            services.AddScopedMock(new Mock<IAuthorizeUserAccessor>());
        });
        var authorizeInteractionService = serviceProvider.GetRequiredService<IAuthorizeInteractionService>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("WebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        consentGrant.ConsentedScopes.Add(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId));
        await AddEntity(consentGrant);

        authenticateUserAccessorMock
            .Setup(x => x.CountAuthenticatedUsers())
            .ReturnsAsync(1)
            .Verifiable();

        authenticateUserAccessorMock
            .Setup(x => x.GetAuthenticatedUser())
            .ReturnsAsync(new AuthenticatedUser(publicSubjectIdentifier.Id))
            .Verifiable();

        // Act
        var deducedPrompt = await authorizeInteractionService.GetPrompt(
            new AuthorizeRequest
            {
                ClientId = client.Id,
                Scope = [ScopeConstants.OpenId]
            }, CancellationToken.None);

        // Assert
        Assert.Equal(PromptConstants.None, deducedPrompt);
        authenticateUserAccessorMock.Verify();
    }
}