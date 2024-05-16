using Domain;

namespace Specs.Helpers.EntityBuilders;
public class AuthorizationCodeBuilder
{
  private readonly AuthorizationCode _authorizationCode;

  private AuthorizationCodeBuilder(string id)
  {
    _authorizationCode = new AuthorizationCode
    {
      Id = id,
      IssuedAt = DateTime.UtcNow
    };
  }

  public static AuthorizationCodeBuilder Instance(string id)
  {
    return new AuthorizationCodeBuilder(id);
  }

  public AuthorizationCode Build()
  {
    return _authorizationCode;
  }

  public AuthorizationCodeBuilder AddCode(string code)
  {
    _authorizationCode.Value = code;
    return this;
  }

  public AuthorizationCodeBuilder AddRedeemed()
  {
    _authorizationCode.RedeemedAt = DateTime.UtcNow;
    _authorizationCode.IsRedeemed = true;
    return this;
  }
}