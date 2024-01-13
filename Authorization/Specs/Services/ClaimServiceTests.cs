using System.Globalization;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Services;
public class ClaimServiceTests : BaseUnitTest
{
  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetClaimsFromConsentGrant_ExpectClaims()
  {
    // Arrange
    var scopes = await IdentityContext.Set<Scope>().ToArrayAsync();
    var claims = await IdentityContext.Set<Claim>().ToArrayAsync();
    var consentGrant = ConsentGrantBuilder
      .Instance()
      .AddClaims(claims)
      .AddScopes(scopes)
      .Build();

    var role = RoleBuilder
      .Instance()
      .AddValue("Admin")
      .Build();

    var user = UserBuilder
      .Instance()
      .AddPassword(CryptographyHelper.GetRandomString(32))
      .AddConsentGrant(consentGrant)
      .AddRole(role)
      .Build();

    var client = ClientBuilder
      .Instance()
      .AddConsentGrant(consentGrant)
      .AddSubjectType(SubjectType.Pairwise)
      .Build();

    var pairwiseIdentifier = PairwiseIdentifierBuilder
      .Instance()
      .AddClient(client)
      .AddUser(user)
      .Build();

    await IdentityContext.AddAsync(pairwiseIdentifier);

    await IdentityContext.SaveChangesAsync();

    var claimService = new ClaimService(IdentityContext);

    // Act
    var consentedClaims = (await claimService.GetClaimsFromConsentGrant(user.Id, client.Id)).ToList();

    // Assert
    Assert.Equal(10, consentedClaims.Count);
    Assert.Equal(pairwiseIdentifier.Id, consentedClaims.Single(c => c.Key == ClaimNameConstants.Sub).Value);
    Assert.Equal(user.GetName(), consentedClaims.Single(c => c.Key == ClaimNameConstants.Name).Value);
    Assert.Equal(user.FirstName, consentedClaims.Single(c => c.Key == ClaimNameConstants.GivenName).Value);
    Assert.Equal(user.LastName, consentedClaims.Single(c => c.Key == ClaimNameConstants.FamilyName).Value);
    Assert.Equal(user.Address, consentedClaims.Single(c => c.Key == ClaimNameConstants.Address).Value);
    Assert.Equal(user.Birthdate.ToString(CultureInfo.InvariantCulture), consentedClaims.Single(c => c.Key == ClaimNameConstants.Birthdate).Value);
    Assert.Equal(user.Locale, consentedClaims.Single(c => c.Key == ClaimNameConstants.Locale).Value);
    Assert.Equal(user.PhoneNumber, consentedClaims.Single(c => c.Key == ClaimNameConstants.Phone).Value);
    Assert.Equal(user.Email, consentedClaims.Single(c => c.Key == ClaimNameConstants.Email).Value);
    Assert.Equal(user.Roles.Select(r => r.Value), consentedClaims.Single(c => c.Key == ClaimNameConstants.Role).Value);
  }
}