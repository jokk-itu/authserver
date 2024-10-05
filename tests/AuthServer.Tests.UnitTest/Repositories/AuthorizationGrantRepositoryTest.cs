using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Repositories;
public class AuthorizationGrantRepositoryTest : BaseUnitTest
{
    public AuthorizationGrantRepositoryTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task CreateAuthorizationGrant_ActiveSessionWithMaxAgeAndPasswordAmr_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Public
        };
        await AddEntity(session);
        await AddEntity(client);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            300,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.Equal(session, authorizationGrant.Session);
        Assert.Equal(publicSubjectIdentifier, authorizationGrant.SubjectIdentifier);
        Assert.Equal(300, authorizationGrant.MaxAge);
        Assert.Single(authorizationGrant.AuthenticationMethodReferences);
        Assert.Equal(AuthenticationMethodReferenceConstants.Password, authorizationGrant.AuthenticationMethodReferences.Single().Name);
    }

    [Fact]
    public async Task CreateAuthorizationGrant_ActiveSessionWithPreviousGrantAndDefaultMaxAge_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Public,
            DefaultMaxAge = 300
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var previousGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(previousGrant);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            null,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.Equal(session, authorizationGrant.Session);
        Assert.Equal(publicSubjectIdentifier, authorizationGrant.SubjectIdentifier);
        Assert.NotNull(previousGrant.RevokedAt);
        Assert.Equal(300, authorizationGrant.MaxAge);
    }

    [Fact]
    public async Task CreateAuthorizationGrant_SubjectTypePairwise_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Pairwise
        };
        await AddEntity(publicSubjectIdentifier);
        await AddEntity(client);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            null,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.NotNull(authorizationGrant.Session);
        Assert.Equal(publicSubjectIdentifier, authorizationGrant.Session.PublicSubjectIdentifier);
        Assert.IsType<PairwiseSubjectIdentifier>(authorizationGrant.SubjectIdentifier);
        Assert.Equal(publicSubjectIdentifier, ((PairwiseSubjectIdentifier)authorizationGrant.SubjectIdentifier).PublicSubjectIdentifier);
        Assert.Null(authorizationGrant.MaxAge);
    }

    [Fact]
    public async Task CreateAuthorizationGrant_ExistingPairwiseSubjectIdentifier_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Pairwise
        };
        var pairwiseSubjectIdentifier = new PairwiseSubjectIdentifier(client, publicSubjectIdentifier);
        await AddEntity(pairwiseSubjectIdentifier);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            null,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.NotNull(authorizationGrant.Session);
        Assert.Equal(publicSubjectIdentifier, authorizationGrant.Session.PublicSubjectIdentifier);
        Assert.Equal(pairwiseSubjectIdentifier, authorizationGrant.SubjectIdentifier);
        Assert.Null(authorizationGrant.MaxAge);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_SubjectAndClientIdWithRevokedGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_SubjectAndClientIdWithExpiredGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr, 0);
        typeof(AuthorizationGrant).GetProperty("AuthTime")!.SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-60));
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_SubjectAndClientIdWithRevokedSession_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        session.Revoke();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_GrantIdWithRevokedGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrant.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_GrantIdWithExpiredGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr, 0);
        typeof(AuthorizationGrant).GetProperty("AuthTime")!.SetValue(authorizationGrant, DateTime.UtcNow.AddSeconds(-60));
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrant.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_GrantIdWithRevokedSession_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var session = new Session(publicSubjectIdentifier);
        session.Revoke();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrant.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }
}
