using System.Net;

namespace Infrastructure.Requests.GetAuthorizationCode;
public class GetAuthorizationCodeResponse : Response
{
  public GetAuthorizationCodeResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetAuthorizationCodeResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string State { get; init; } = null!;

  public string Code { get; init; } = null!;
}
