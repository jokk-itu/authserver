using Domain;

namespace Specs.Helpers.EntityBuilders;
public class AuthorizationCodeGrantBuilder
{
  private readonly AuthorizationCodeGrant _authorizationCodeGrant;

  private AuthorizationCodeGrantBuilder(string id)
  {
    _authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = id,
      AuthTime = DateTime.UtcNow,
    };
  }

  public static AuthorizationCodeGrantBuilder Instance(string id)
  {
    return new AuthorizationCodeGrantBuilder(id);
  }

  public AuthorizationCodeGrant Build()
  {
    return _authorizationCodeGrant;
  }

  public AuthorizationCodeGrantBuilder AddToken(GrantToken token)
  {
    _authorizationCodeGrant.GrantTokens.Add(token);
    return this;
  } 

  public AuthorizationCodeGrantBuilder AddRevoked()
  {
    _authorizationCodeGrant.IsRevoked = true;
    return this;
  }

  public AuthorizationCodeGrantBuilder AddNonce(Nonce nonce)
  {
    _authorizationCodeGrant.Nonces.Add(nonce);
    return this;
  }

  public AuthorizationCodeGrantBuilder AddAuthorizationCode(AuthorizationCode authorizationCode)
  {
    _authorizationCodeGrant.AuthorizationCodes.Add(authorizationCode);
    return this;
  }

  public AuthorizationCodeGrantBuilder AddClient(Client client)
  {
    _authorizationCodeGrant.Client = client;
    return this;
  }
}
