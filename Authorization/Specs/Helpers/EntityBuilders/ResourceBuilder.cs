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
      Name = faker.Name.FirstName(),
      Uri = "https://weather.authserver.dk"
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

  public ResourceBuilder AddSecret(string secret)
  {
    var hashedSecret = BCrypt.HashPassword(secret, BCrypt.GenerateSalt());
    _resource.Secret = hashedSecret;
    return this;
  }
}