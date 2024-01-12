using System.Net;

namespace Infrastructure.Requests.GetUserInfo;

public class GetUserInfoResponse : Response
{
  public GetUserInfoResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetUserInfoResponse(string errorCode, string errorDescription, HttpStatusCode statusCode)
    : base(errorCode, errorDescription, statusCode)
  {
  }

  public IEnumerable<KeyValuePair<string, object>> UserInfo { get; init; } = new List<KeyValuePair<string, object>>();
}