using System.Net;

namespace Infrastructure.Requests.GetLoginToken;

#nullable disable
public class GetLoginTokenResponse : Response
{
  public GetLoginTokenResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetLoginTokenResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string LoginToken { get; init; }
}
