using Domain;

namespace Specs.Helpers.EntityBuilders;
public class RoleBuilder
{
  private readonly Role _role;

  private RoleBuilder()
  {
    _role = new Role();
  }

  public static RoleBuilder Instance()
  {
    return new RoleBuilder();
  }

  public Role Build()
  {
    return _role;
  }

  public RoleBuilder AddValue(string value)
  {
    _role.Value = value;
    return this;
  }
}