using Domain;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Specs.Helpers.Builders;
using Xunit;

namespace Specs.Services;
public class ClaimServiceTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetClaimsFromConsentGrant_ConsentGrantIsNull_ExpectOne()
  {
    // Arrange
    var claimService = new ClaimService(IdentityContext);
    var clientId = Guid.NewGuid().ToString();
    var userId = Guid.NewGuid().ToString();

    // Act
    var claims = await claimService.GetClaimsFromConsentGrant(userId, clientId);

    // Assert
    Assert.Single(claims);
    Assert.Equal(userId, claims.First().Value);
  }

  [Fact]
  public async Task GetClaimsFromConsentGrant_ExpectClaims()
  {
    // Arrange
    var scopes = await IdentityContext.Set<Scope>().ToListAsync();
    var claims = await IdentityContext.Set<Claim>().ToListAsync();
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddClaims(claims)
      .AddScopes(scopes)
      .Build();

    var user = UserBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .Build();

    await IdentityContext
      .Set<User>()
      .AddAsync(user);
    await IdentityContext
      .Set<Client>()
      .AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var claimService = new ClaimService(IdentityContext);

    // Act
    var consentedClaims = await claimService.GetClaimsFromConsentGrant(user.Id, client.Id);

    // Assert
    Assert.Equal(9, consentedClaims.Count);
  }
}