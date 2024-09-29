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
    public async Task CreateAuthorizationGrant_ActiveSessionWithMaxAge_ExpectGrant()
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
        var grant = await authorizationGrantRepository.CreateAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, 300, CancellationToken.None);

        // Assert
        Assert.Equal(client, grant.Client);
        Assert.Equal(session, grant.Session);
        Assert.Equal(publicSubjectIdentifier, grant.SubjectIdentifier);
        Assert.Equal(300, grant.MaxAge);
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
        var previousGrant = new AuthorizationGrant(session, client, publicSubjectIdentifier);
        await AddEntity(previousGrant);

        // Act
        var grant = await authorizationGrantRepository.CreateAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, null, CancellationToken.None);

        // Assert
        Assert.Equal(client, grant.Client);
        Assert.Equal(session, grant.Session);
        Assert.Equal(publicSubjectIdentifier, grant.SubjectIdentifier);
        Assert.NotNull(previousGrant.RevokedAt);
        Assert.Equal(300, grant.MaxAge);
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
        var grant = await authorizationGrantRepository.CreateAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, null, CancellationToken.None);

        // Assert
        Assert.Equal(client, grant.Client);
        Assert.NotNull(grant.Session);
        Assert.Equal(publicSubjectIdentifier, grant.Session.PublicSubjectIdentifier);
        Assert.IsType<PairwiseSubjectIdentifier>(grant.SubjectIdentifier);
        Assert.Equal(publicSubjectIdentifier, ((PairwiseSubjectIdentifier)grant.SubjectIdentifier).PublicSubjectIdentifier);
        Assert.Null(grant.MaxAge);
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
        var grant = await authorizationGrantRepository.CreateAuthorizationGrant(publicSubjectIdentifier.Id, client.Id, null, CancellationToken.None);

        // Assert
        Assert.Equal(client, grant.Client);
        Assert.NotNull(grant.Session);
        Assert.Equal(publicSubjectIdentifier, grant.Session.PublicSubjectIdentifier);
        Assert.Equal(pairwiseSubjectIdentifier, grant.SubjectIdentifier);
        Assert.Null(grant.MaxAge);
    }
}
