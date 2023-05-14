using Domain;

namespace Specs.Helpers.EntityBuilders;
public abstract class TokenBuilder
{
  protected string _scope = string.Empty;
  protected DateTime _expiresAt = DateTime.UtcNow.AddHours(1);
  protected DateTime _issuedAt = DateTime.UtcNow;
  protected DateTime _notBefore = DateTime.UtcNow;
  protected string _audience = string.Empty;
  protected string _issuer = string.Empty;
  protected DateTime? _revokedAt;

  public abstract Token Build();

  public TokenBuilder AddScope(string scope)
  {
    _scope = scope;
    return this;
  }

  public TokenBuilder AddExpiresAt(DateTime expiresAt)
  {
    _expiresAt = expiresAt;
    return this;
  }

  public TokenBuilder AddAudience(string audience)
  {
    _audience = audience;
    return this;
  }

  public TokenBuilder AddIssuer(string issuer)
  {
    _issuer = issuer;
    return this;
  }

  public TokenBuilder AddRevokedAt(DateTime revokedAt)
  {
    _revokedAt = revokedAt;
    return this;
  }
}
