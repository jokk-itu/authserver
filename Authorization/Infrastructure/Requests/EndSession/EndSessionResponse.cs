using System.Net;

namespace Infrastructure.Requests.EndSession;
public class EndSessionResponse : Response
{
  public EndSessionResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public EndSessionResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}