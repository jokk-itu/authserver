namespace Infrastructure.Builders.Token.GrantAccessToken;
#nullable disable
public class GrantAccessTokenArguments
{
  public IEnumerable<string> ResourceNames { get; init; } = new List<string>();
  public string Scope { get; init; }
  public string AuthorizationGrantId { get; init; }
}