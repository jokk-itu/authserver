using System.Net;

namespace Infrastructure.Requests.CreateConsentGrant;
public class CreateConsentGrantResponse : Response
{
  public CreateConsentGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateConsentGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}