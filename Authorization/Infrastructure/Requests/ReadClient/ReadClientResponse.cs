using System.Net;
using Domain;

namespace Infrastructure.Requests.ReadClient;
public class ReadClientResponse : Response
{
  public ReadClientResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public ReadClientResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string ClientId { get; init; } = null!;
  public string? ClientSecret { get; init; }
  public string ClientName { get; init; } = null!;
  public string ApplicationType { get; init; } = null!;
  public string TokenEndpointAuthMethod { get; init; } = null!;
  public string SubjectType { get; init; } = null!;
  public string? TosUri { get; init; }
  public string? PolicyUri { get; init; }
  public string? InitiateLoginUri { get; init; }
  public string? LogoUri { get; init; }
  public string? ClientUri { get; init; }
  public long? DefaultMaxAge { get; init; }
  public string RegistrationAccessToken { get; init; } = null!;
}
