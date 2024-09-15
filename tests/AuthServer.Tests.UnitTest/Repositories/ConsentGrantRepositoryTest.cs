using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.UnitTest.Repositories;

public class ConsentGrantRepositoryTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task GetConsentedScope_NoConsentGrant_ExpectEmpty()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        // Act
        var consentedScope = await consentGrantRepository.GetConsentedScope(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(), CancellationToken.None);

        // Assert
        Assert.Empty(consentedScope);
    }

    [Fact]
    public async Task GetConsentedScope_ConsentGrantExists_ExpectOneScope()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        var openIdScope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
        consentGrant.ConsentedScopes.Add(openIdScope);
        await AddEntity(consentGrant);

        // Act
        var consentedScope=
            await consentGrantRepository.GetConsentedScope(publicSubjectIdentifier.Id, client.Id,
                CancellationToken.None);

        // Assert
        Assert.Equal(openIdScope.Name, consentedScope.Single());
    }

    [Fact]
    public async Task GetConsentedClaims_ConsentGrantDoesNotExist_ExpectEmpty()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        await AddEntity(consentGrant);

        // Act
        var consentedClaims =
            await consentGrantRepository.GetConsentedClaims(publicSubjectIdentifier.Id, client.Id,
                CancellationToken.None);

        // Assert
        Assert.Empty(consentedClaims);
    }

    [Fact]
    public async Task GetConsentedClaims_ConsentGrantExists_ExpectOneClaim()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        var givenNameClaim = await IdentityContext.Set<Claim>().SingleAsync(x => x.Name == ClaimNameConstants.GivenName);
        consentGrant.ConsentedClaims.Add(givenNameClaim);
        await AddEntity(consentGrant);

        // Act
        var consentedClaims =
            await consentGrantRepository.GetConsentedClaims(publicSubjectIdentifier.Id, client.Id,
                CancellationToken.None);

        // Assert
        Assert.Equal(givenNameClaim.Name, consentedClaims.Single());
    }

    [Fact]
    public async Task GetConsentedClaims_ClientDoesNotRequireConsent_ExpectAllClaims()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic)
        {
            RequireConsent = false
        };
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);
        await AddEntity(consentGrant);

        // Act
        var consentedClaims =
            await consentGrantRepository.GetConsentedClaims(publicSubjectIdentifier.Id, client.Id,
                CancellationToken.None);

        // Assert
        var allClaims = IdentityContext.Set<Claim>().Select(x => x.Name).ToList();
        Assert.Equal(consentedClaims, allClaims);
    }

    [Fact]
    public async Task CreateOrUpdateConsentGrant_NoExistingConsent_ExpectNewConsent()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        await AddEntity(client);
        await AddEntity(publicSubjectIdentifier);

        // Act
        await consentGrantRepository.CreateOrUpdateConsentGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            [ScopeConstants.OpenId],
            [ClaimNameConstants.GivenName],
            CancellationToken.None);

        // Assert
        var consentGrant = await IdentityContext
            .Set<ConsentGrant>()
            .SingleAsync();

        Assert.Equal(client, consentGrant.Client);
        Assert.Equal(publicSubjectIdentifier, consentGrant.PublicSubjectIdentifier);
        Assert.Equal(IdentityContext.Set<Scope>().Single(x => x.Name == ScopeConstants.OpenId), consentGrant.ConsentedScopes.Single());
        Assert.Equal(IdentityContext.Set<Claim>().Single(x => x.Name == ClaimNameConstants.GivenName), consentGrant.ConsentedClaims.Single());
    }

    [Fact]
    public async Task CreateOrUpdateConsentGrant_ExistingConsent_ExpectUpdatedConsent()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);

        var openIdScope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId);
        consentGrant.ConsentedScopes.Add(openIdScope);

        var givenNameClaim = await IdentityContext.Set<Claim>().SingleAsync(x => x.Name == ClaimNameConstants.GivenName);
        consentGrant.ConsentedClaims.Add(givenNameClaim);

        await AddEntity(consentGrant);

        // Act
        await consentGrantRepository.CreateOrUpdateConsentGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            [ScopeConstants.UserInfo],
            [ClaimNameConstants.Address],
            CancellationToken.None);

        // Assert
        Assert.Equal(await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.UserInfo), consentGrant.ConsentedScopes.Single());
        Assert.Equal(await IdentityContext.Set<Claim>().SingleAsync(x => x.Name == ClaimNameConstants.Address), consentGrant.ConsentedClaims.Single());
    }

    [Fact]
    public async Task GetConsentGrant_ExistingGrant_ExpectConsentGrant()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();
        var consentGrant = new ConsentGrant(publicSubjectIdentifier, client);

        await AddEntity(consentGrant);

        // Act
        var grant = await consentGrantRepository.GetConsentGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            CancellationToken.None);

        // Assert
        Assert.Equal(consentGrant, grant);
    }

    [Fact]
    public async Task GetConsentGrant_NotExistingGrant_ExpectNull()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var consentGrantRepository = serviceProvider.GetRequiredService<IConsentGrantRepository>();

        var client = new Client("PinguApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        var publicSubjectIdentifier = new PublicSubjectIdentifier();

        await AddEntity(client);
        await AddEntity(publicSubjectIdentifier);

        // Act
        var grant = await consentGrantRepository.GetConsentGrant(
            publicSubjectIdentifier.Id,
            client.Id,
            CancellationToken.None);

        // Assert
        Assert.Null(grant);
    }
}