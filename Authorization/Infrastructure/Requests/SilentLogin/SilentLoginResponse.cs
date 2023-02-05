using System.Net;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginResponse : Response
{
  public SilentLoginResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public SilentLoginResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string UserId { get; init; }
}
