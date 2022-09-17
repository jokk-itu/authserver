using System.Net;

namespace Infrastructure.Requests.CreateClient;
public class CreateClientResponse : Response
{
  public ICollection<string> RedirectUris { get; init; } = new List<string>();

  public ICollection<string> ResponseTypes { get; init; } = new List<string>();

  public ICollection<string> GrantTypes { get; init; } = new List<string>();
  public string ApplicationType { get; init; } = null!;
  public ICollection<string> Contacts { get; init; } = new List<string>();
  public string ClientName { get; init; }= null!;
  public string PolicyUri { get; init; }= null!;
  public string TosUri { get; init; }= null!;
  public string SubjectType { get; init; }= null!;
  public string TokenEndpointAuthMethod { get; init; }= null!;
  public string Scope { get; init; } = null!;
  public string ClientId { get; init; } = null!;
  public string ClientSecret { get; init; } = null!;
  public string RegistrationAccessToken { get; init; } = null!;

  public CreateClientResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateClientResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}