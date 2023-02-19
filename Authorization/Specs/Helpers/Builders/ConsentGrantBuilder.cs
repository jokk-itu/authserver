using Domain;

namespace Specs.Helpers.Builders;
public class ConsentGrantBuilder
{
  private readonly ConsentGrant _consentGrant;

  private ConsentGrantBuilder()
  {
    _consentGrant = new ConsentGrant
    {
      Updated = DateTime.UtcNow
    };
  }

  public static ConsentGrantBuilder Instance()
  {
    return new ConsentGrantBuilder();
  }

  public ConsentGrant Build()
  {
    return _consentGrant;
  }

  public ConsentGrantBuilder AddClaims(IEnumerable<Claim> claims)
  {
    foreach (var claim in claims)
    {
      _consentGrant.ConsentedClaims.Add(claim); 
    }

    return this;
  }

  public ConsentGrantBuilder AddScopes(IEnumerable<Scope> scopes)
  {
    foreach (var scope in scopes)
    {
      _consentGrant.ConsentedScopes.Add(scope); 
    }

    return this;
  }
}
