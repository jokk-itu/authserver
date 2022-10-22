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

  public string ClientId { get; init; }
  public string ClientName { get; init; }
  public string ClientSecret { get; init; }
  public string Scope { get; init; }
  public string TosUri { get; init; }
  public string PolicyUri { get; init; }
  public string TokenEndpointAuthMethod { get; init; }
  public string SubjectType { get; init; }
  public string ApplicationType { get; init; }
  public ICollection<string> RedirectUris { get; init; }
  public ICollection<Domain.GrantType> GrantTypes { get; init; }
  public ICollection<Contact> Contacts { get; init; }
  public ICollection<string> ResponseTypes { get; init; }
}
