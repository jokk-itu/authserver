namespace Infrastructure.Builders.Token.GrantAccessToken;
#nullable disable
public class GrantAccessTokenArguments
{
  public IEnumerable<string> Resource { get; init; } = new List<string>();
  public string Scope { get; init; }
  public string AuthorizationGrantId { get; init; }
}