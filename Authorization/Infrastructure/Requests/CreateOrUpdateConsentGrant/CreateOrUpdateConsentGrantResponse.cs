using System.Net;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;
public class CreateOrUpdateConsentGrantResponse : Response
{
  public CreateOrUpdateConsentGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateOrUpdateConsentGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}