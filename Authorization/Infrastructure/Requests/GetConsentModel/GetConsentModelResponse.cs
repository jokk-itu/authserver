using System.Net;

namespace Infrastructure.Requests.GetConsentModel;
public class GetConsentModelResponse : Response
{
  public GetConsentModelResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetConsentModelResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public IEnumerable<ClaimDto> Claims { get; init; } = new List<ClaimDto>();
  public IEnumerable<ScopeDto> Scopes { get; init; } = new List<ScopeDto>();
  public string ClientName { get; init; } = null!;
  public string GivenName { get; init; } = null!;
  public string? TosUri { get; init; }
  public string? PolicyUri { get; init; }
  public string? LogoUri { get; init; }
}
