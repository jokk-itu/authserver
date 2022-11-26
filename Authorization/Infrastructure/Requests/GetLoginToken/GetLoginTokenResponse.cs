using System.Net;

namespace Infrastructure.Requests.GetLoginToken;

public class GetLoginTokenResponse : Response
{
  public GetLoginTokenResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetLoginTokenResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string LoginCode { get; init; } = null!;
}
