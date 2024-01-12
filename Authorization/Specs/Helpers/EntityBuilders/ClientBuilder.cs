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
      PolicyUri = "https://localhost:5001/policy",
      TosUri = "https://locaolhost:5001/tos",
      ClientUri = "https://localhost:5001"
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

  public ClientBuilder AddSubjectType(SubjectType subjectType)
  {
    _client.SubjectType = subjectType;
    return this;
  }

  public ClientBuilder AddClientUri(string clientUri)
  {
    _client.ClientUri = clientUri;
    return this;
  }

  public ClientBuilder AddSecret(string secret)
  {
    var hashedSecret = BCrypt.HashPassword(secret, BCrypt.GenerateSalt());
    _client.Secret = hashedSecret;
    return this;
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

  public ClientBuilder AddScopes(params Scope[] scopes)
  {
    scopes.ToList().ForEach(_client.Scopes.Add);
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
    _client.BackchannelLogoutUri = uri;
    return this;
  }
}
