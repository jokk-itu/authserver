using System.Net;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientResponse : Response
{
  public ICollection<string> RedirectUris { get; init; } = new List<string>();
  public ICollection<string> PostLogoutRedirectUris { get; init; } = new List<string>();
  public ICollection<string> ResponseTypes { get; init; } = new List<string>();
  public ICollection<string> GrantTypes { get; init; } = new List<string>();
  public string ApplicationType { get; init; } = null!;
  public ICollection<string> Contacts { get; init; } = new List<string>();
  public string ClientName { get; init; } = null!;
  public string? PolicyUri { get; init; }
  public string? TosUri { get; init; }
  public string SubjectType { get; init; } = null!;
  public string TokenEndpointAuthMethod { get; init; } = null!;
  public string Scope { get; init; } = null!;
  public string ClientId { get; init; } = null!;
  public string? ClientSecret { get; init; }
  public string RegistrationAccessToken { get; init; } = null!;
  public string RegistrationClientUri { get; init; } = null!;
  public long? DefaultMaxAge { get; init; }
  public string? InitiateLoginUri { get; init; }
  public string? LogoUri { get; init; }
  public string? ClientUri { get; init; }
  public int ClientSecretExpiresAt { get; init; }
  public long ClientIdIssuedAt { get; init; }

  public CreateClientResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateClientResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}