using Domain;

namespace Specs.Helpers.Builders;
public class SessionBuilder
{
  private readonly Session _session;

  public SessionBuilder()
  {
    _session = new Session
    {
      Created = DateTime.UtcNow,
      MaxAge = 0,
      Updated = DateTime.UtcNow
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

  public SessionBuilder AddAuthorizationCodeGrant(AuthorizationCodeGrant authorizationCodeGrant)
  {
    _session.AuthorizationCodeGrants.Add(authorizationCodeGrant);
    return this;
  }
}
