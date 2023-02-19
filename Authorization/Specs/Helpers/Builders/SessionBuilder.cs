using Domain;

namespace Specs.Helpers.Builders;
public class SessionBuilder
{
  private readonly Session _session;

  public SessionBuilder()
  {
    _session = new Session
    {
      IsRevoked = false
    };
  }

  public static SessionBuilder Instance()
  {
    return new SessionBuilder();
  }

  public Session Build()
  {
    return _session;
  }

  public SessionBuilder RevokeSession()
  {
    _session.IsRevoked = true;
    return this;
  }

  public SessionBuilder AddAuthorizationCodeGrant(AuthorizationCodeGrant authorizationCodeGrant)
  {
    _session.AuthorizationCodeGrants.Add(authorizationCodeGrant);
    return this;
  }
}
