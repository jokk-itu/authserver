using AuthServer.Constants;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AuthServer.Tests.Unit.Repositories;

public class ConsentGrantRepositoryTest(ITestOutputHelper outputHelper) : BaseUnitTest(outputHelper)
{
    [Fact]
    public async Task GetConsentedScope_NoConsentGrant_Empty()
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
    public async Task GetConsentedScope_ConsentGrantExists_OneScope()
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
        var consentedScope =
            await consentGrantRepository.GetConsentedScope(publicSubjectIdentifier.Id, client.Id,
                CancellationToken.None);

        // Assert
        Assert.Equal(openIdScope.Name, consentedScope.Single());
    }
}