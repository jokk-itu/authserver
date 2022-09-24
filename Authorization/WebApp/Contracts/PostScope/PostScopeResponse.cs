namespace WebApp.Contracts.PostScope;

#nullable disable
public class PostScopeResponse
{
  public int Id { get; init; }

  public string ScopeName { get; init; }

  public string ScopeRegistrationAccessToken { get; init; }
}