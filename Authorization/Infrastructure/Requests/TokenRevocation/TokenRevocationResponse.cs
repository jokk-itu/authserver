using System.Net;

namespace Infrastructure.Requests.TokenRevocation;
public class TokenRevocationResponse : Response
{
  public TokenRevocationResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public TokenRevocationResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}
