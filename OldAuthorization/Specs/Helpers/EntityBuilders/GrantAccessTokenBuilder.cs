using Domain;

namespace Specs.Helpers.EntityBuilders;
public class GrantAccessTokenBuilder : TokenBuilder
{
  public static GrantAccessTokenBuilder Instance()
  {
    return new GrantAccessTokenBuilder();
  }

  public override GrantAccessToken Build()
  {
    return new GrantAccessToken
    {
      Scope = _scope,
      RevokedAt = _revokedAt,
      Audience = _audience,
      ExpiresAt = _expiresAt,
      IssuedAt = _issuedAt,
      NotBefore = _notBefore,
      Issuer = _issuer
    };
  }
}
