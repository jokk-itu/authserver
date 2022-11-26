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
  public string ClientName { get; init; } = null!;
  public string ClientSecret { get; init; } = null!;
  public string Scope { get; init; } = null!;
  public string TosUri { get; init; } = string.Empty;
  public string PolicyUri { get; init; } = string.Empty;
  public string TokenEndpointAuthMethod { get; init; } = null!;
  public string SubjectType { get; init; } = null!;
  public string ApplicationType { get; init; } = null!;
  public ICollection<string> RedirectUris { get; init; } = new List<string>();
  public ICollection<GrantType> GrantTypes { get; init; } = new List<GrantType>();
  public ICollection<Contact> Contacts { get; init; } = new List<Contact>();
  public ICollection<string> ResponseTypes { get; init; } = new List<string>();
}
