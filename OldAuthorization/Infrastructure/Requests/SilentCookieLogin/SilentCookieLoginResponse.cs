using System.Net;

#nullable disable
namespace Infrastructure.Requests.SilentCookieLogin;
public class SilentCookieLoginResponse : Response
{
  public SilentCookieLoginResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public SilentCookieLoginResponse(string errorCode, string errorDescription, HttpStatusCode statusCode)
    : base(errorCode, errorDescription, statusCode)
  {
  }

  public string Code { get; init; }
}