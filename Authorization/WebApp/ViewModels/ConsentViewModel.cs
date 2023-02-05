using Infrastructure.Requests.GetConsentModel;

namespace WebApp.ViewModels;

#nullable enable
public class ConsentViewModel
{
  public string ClientName { get; init; } = null!;
  public string GivenName { get; init; } = null!;
  public IEnumerable<ClaimDto> Claims { get; init; } = new List<ClaimDto>();
  public string? TosUri { get; init; }
  public string? PolicyUri { get; init; }
  public string FormMethod { get; init; } = null!;
}