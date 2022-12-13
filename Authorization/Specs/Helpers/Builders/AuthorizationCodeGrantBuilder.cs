using Domain;
using Infrastructure.Helpers;

namespace Specs.Helpers.Builders;
public class AuthorizationCodeGrantBuilder
{
  private readonly AuthorizationCodeGrant _authorizationCodeGrant;

  private AuthorizationCodeGrantBuilder()
  {
    _authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = Guid.NewGuid().ToString(),
      AuthTime = DateTime.UtcNow,
      IsRedeemed = false,
      Nonce = CryptographyHelper.GetRandomString(16)
    };
  }

  public static AuthorizationCodeGrantBuilder Instance()
  {
    return new AuthorizationCodeGrantBuilder();
  }

  public AuthorizationCodeGrant Build()
  {
    return _authorizationCodeGrant;
  }

  public AuthorizationCodeGrantBuilder AddCode(string code)
  {
    _authorizationCodeGrant.Code = code;
    return this;
  }

  public AuthorizationCodeGrantBuilder IsRedeemed()
  {
    _authorizationCodeGrant.IsRedeemed = true;
    return this;
  }

  public AuthorizationCodeGrantBuilder AddClient(Client client)
  {
    _authorizationCodeGrant.Client = client;
    return this;
  }
}
