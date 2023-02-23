using Bogus;
using Domain;

namespace Specs.Helpers.Builders;
public class ScopeBuilder
{
  private readonly Scope _scope;

  private ScopeBuilder()
  {
    var faker = new Faker();
    _scope = new Scope
    {
      Name = faker.Name.FirstName()
    };
  }

  public static ScopeBuilder Instance()
  {
    return new ScopeBuilder();
  }

  public Scope Build()
  {
    return _scope;
  }
}
