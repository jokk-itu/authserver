using Bogus;
using Domain;
using Domain.Enums;
using Infrastructure.Helpers;

namespace Specs.Helpers.EntityBuilders;
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
      PolicyUri = "https://localhost:5001/policy",
      TosUri = "https://locaolhost:5001/tos"
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

  public ClientBuilder AddTokenEndpointAuthMethod(TokenEndpointAuthMethod tokenEndpointAuthMethod)
  {
    _client.TokenEndpointAuthMethod = tokenEndpointAuthMethod;
    return this;
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

  public ClientBuilder AddRedirectUri(string uri)
  {
    _client.RedirectUris.Add(new RedirectUri
    {
      Type = RedirectUriType.AuthorizeRedirectUri,
      Uri = uri
    });
    return this;
  }

  public ClientBuilder AddApplicationType(ApplicationType applicationType)
  {
    _client.ApplicationType = applicationType;
    return this;
  }

  public ClientBuilder AddPostLogoutRedirectUri(string uri)
  {
    _client.RedirectUris.Add(new RedirectUri()
    {
      Type = RedirectUriType.PostLogoutRedirectUri,
      Uri = uri
    });
    return this;
  }

  public ClientBuilder AddBackChannelLogoutUri(string uri)
  {
    _client.BackChannelLogoutUri = uri;
    return this;
  }
}
