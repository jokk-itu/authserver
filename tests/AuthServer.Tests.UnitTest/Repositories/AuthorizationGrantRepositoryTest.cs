using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
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
    public async Task CreateAuthorizationGrant_ActiveSessionWithPasswordAmr_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Public
        };
        await AddEntity(session);
        await AddEntity(client);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            subjectIdentifier.Id,
            client.Id,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.Equal(session, authorizationGrant.Session);
        Assert.Equal(subjectIdentifier.Id, authorizationGrant.Subject);
        Assert.Single(authorizationGrant.AuthenticationMethodReferences);
        Assert.Equal(AuthenticationMethodReferenceConstants.Password, authorizationGrant.AuthenticationMethodReferences.Single().Name);
    }

    [Fact]
    public async Task CreateAuthorizationGrant_ActiveSessionWithPreviousGrant_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Public
        };
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var previousGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(previousGrant);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            subjectIdentifier.Id,
            client.Id,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.Equal(session, authorizationGrant.Session);
        Assert.Equal(subjectIdentifier.Id, authorizationGrant.Subject);
        Assert.NotNull(previousGrant.RevokedAt);
    }

    [Fact]
    public async Task CreateAuthorizationGrant_SubjectTypePairwise_ExpectGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();
        var subjectIdentifier = new SubjectIdentifier();
        var sectorIdentifier = new SectorIdentifier("https://sector.authserver.dk/uris.json");
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            SubjectType = SubjectType.Pairwise,
            SectorIdentifier = sectorIdentifier
        };
        await AddEntity(subjectIdentifier);
        await AddEntity(client);

        // Act
        var authorizationGrant = await authorizationGrantRepository.CreateAuthorizationGrant(
            subjectIdentifier.Id,
            client.Id,
            LevelOfAssuranceLow,
            [AuthenticationMethodReferenceConstants.Password],
            CancellationToken.None);

        // Assert
        Assert.Equal(client, authorizationGrant.Client);
        Assert.NotNull(authorizationGrant.Session);
        Assert.Equal(PairwiseSubjectHelper.GenerateSubject(sectorIdentifier, subjectIdentifier.Id), authorizationGrant.Subject);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_SubjectAndClientIdWithRevokedGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        authorizationGrant.Revoke();
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier.Id, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_SubjectAndClientIdWithRevokedSession_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        session.Revoke();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(subjectIdentifier.Id, client.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }

    [Fact]
    public async Task GetActiveAuthorizationGrant_GrantIdWithRevokedGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var authorizationGrantRepository = serviceProvider.GetRequiredService<IAuthorizationGrantRepository>();

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        authorizationGrant.Revoke();
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

        var subjectIdentifier = new SubjectIdentifier();
        var session = new Session(subjectIdentifier);
        session.Revoke();

        var client = new Client("webapp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var lowAcr = await GetAuthenticationContextReference(LevelOfAssuranceLow);
        var authorizationGrant = new AuthorizationGrant(session, client, subjectIdentifier.Id, lowAcr);
        await AddEntity(authorizationGrant);

        // Act
        var activeGrant = await authorizationGrantRepository.GetActiveAuthorizationGrant(authorizationGrant.Id, CancellationToken.None);

        // Assert
        Assert.Null(activeGrant);
    }
}
