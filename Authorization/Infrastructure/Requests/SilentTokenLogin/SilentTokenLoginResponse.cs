#nullable disable
using System.Net;

namespace Infrastructure.Requests.SilentTokenLogin;
public class SilentTokenLoginResponse : Response
{
  public SilentTokenLoginResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public SilentTokenLoginResponse(string errorCode, string errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string Code { get; init; }
}
