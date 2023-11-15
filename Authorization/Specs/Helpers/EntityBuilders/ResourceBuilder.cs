using Bogus;
using Domain;
using Infrastructure.Helpers;

namespace Specs.Helpers.EntityBuilders;

public class ResourceBuilder
{
  private readonly Resource _resource;

  private ResourceBuilder()
  {
    var faker = new Faker();
    _resource = new Resource
    {
      Id = Guid.NewGuid().ToString(),
      Secret = CryptographyHelper.GetRandomString(16),
      Name = faker.Name.FirstName(),
      Uri = "https://localhost:5000"
    };
  }

  public static ResourceBuilder Instance()
  {
    return new ResourceBuilder();
  }

  public Resource Build()
  {
    return _resource;
  }

  public ResourceBuilder AddScope(Scope scope)
  {
    _resource.Scopes.Add(scope);
    return this;
  }
}