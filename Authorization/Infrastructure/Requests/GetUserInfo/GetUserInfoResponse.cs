using System.Net;

namespace Infrastructure.Requests.GetUserInfo;

public class GetUserInfoResponse : Response
{
  public GetUserInfoResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetUserInfoResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public IDictionary<string, string> UserInfo { get; init; } = new Dictionary<string, string>();
}