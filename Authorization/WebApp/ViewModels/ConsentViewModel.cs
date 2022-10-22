namespace WebApp.ViewModels;

#nullable disable
public class ConsentViewModel
{
  public string ClientName { get; init; }
  public ICollection<string> Scopes { get; init; }
  public ICollection<string> Claims { get; init; }
  public string Query { get; init; }
  public string AccessToken { get; init; }
}