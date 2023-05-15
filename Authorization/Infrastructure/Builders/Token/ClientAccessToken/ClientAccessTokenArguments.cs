namespace Infrastructure.Builders.Token.ClientAccessToken;
#nullable disable
public class ClientAccessTokenArguments
{
  public IEnumerable<string> ResourceNames { get; init; } = new List<string>();
  public string Scope { get; init; }
  public string ClientId { get; init; }
}