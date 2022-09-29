using System.Net;

namespace Infrastructure.Requests.HasUserConsent;
public class HasUserConsentResponse : Response
{
  public HasUserConsentResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public HasUserConsentResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public bool HasValidConsent { get; set; }
}
