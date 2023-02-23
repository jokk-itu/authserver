using Bogus;
using Domain;
using Domain.Enums;
using Infrastructure.Helpers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Specs.Helpers.Builders;
public class ClientBuilder
{
  private readonly Client _client;

  private ClientBuilder()
  {
    var faker = new Faker();
    _client = new Client
    {
      Id = Guid.NewGuid().ToString(),
      ApplicationType = ApplicationType.Web,
      Name = faker.Name.FirstName(),
      Secret = CryptographyHelper.GetRandomString(32),
      TokenEndpointAuthMethod = TokenEndpointAuthMethod.ClientSecretPost,
      PolicyUri = faker.Person.Website,
      TosUri = faker.Person.Website
    };
  }

  public static ClientBuilder Instance()
  {
    return new ClientBuilder();
  }

  public Client Build()
  {
    return _client;
  }

  public ClientBuilder AddConsentGrant(ConsentGrant consentGrant)
  {
    _client.ConsentGrants.Add(consentGrant);
    return this;
  }

  public ClientBuilder AddGrantType(GrantType grantType)
  {
    _client.GrantTypes.Add(grantType);
    return this;
  }

  public ClientBuilder AddScope(Scope scope)
  {
    _client.Scopes.Add(scope);
    return this;
  }

  public ClientBuilder AddRedirect(RedirectUri redirectUri)
  {
    _client.RedirectUris.Add(redirectUri);
    return this;
  }
}
