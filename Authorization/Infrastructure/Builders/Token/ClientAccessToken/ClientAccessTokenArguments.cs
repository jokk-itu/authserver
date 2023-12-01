namespace Infrastructure.Builders.Token.ClientAccessToken;
#nullable disable
public class ClientAccessTokenArguments
{
  public ICollection<string> Resource { get; init; } = new List<string>();
  public string Scope { get; init; }
  public string ClientId { get; init; }
}