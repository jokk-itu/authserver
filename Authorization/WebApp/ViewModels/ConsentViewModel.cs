namespace WebApp.ViewModels;

#nullable enable
public class ConsentViewModel
{
  public string ClientName { get; init; } = null!;
  public string GivenName { get; init; } = null!;
  public IEnumerable<string> Scopes { get; init; } = new List<string>();
  public IEnumerable<string> Claims { get; init; } = new List<string>();
  public string? TosUri { get; init; }
  public string? PolicyUri { get; init; }
  public string FormMethod { get; init; } = null!;
}