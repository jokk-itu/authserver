using System.Net;

namespace Infrastructure.Requests.GeUserInfo;

public class GetUserInfoResponse : Response
{
  public GetUserInfoResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public GetUserInfoResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public IEnumerable<(string, string)> UserInfo { get; init; } = new List<(string, string)>();
}
