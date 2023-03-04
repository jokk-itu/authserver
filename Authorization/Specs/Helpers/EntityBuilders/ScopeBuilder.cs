using Domain;

namespace Specs.Helpers.EntityBuilders;
public class ScopeBuilder
{
  private readonly Scope _scope;

  private ScopeBuilder()
  {
    _scope = new Scope();
  }

  public static ScopeBuilder Instance()
  {
    return new ScopeBuilder();
  }

  public Scope Build()
  {
    return _scope;
  }

  public ScopeBuilder AddName(string name)
  {
    _scope.Name = name;
    return this;
  }
}
