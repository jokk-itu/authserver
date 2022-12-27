using System.Net;

namespace Infrastructure.Requests.CreateRefreshTokenGrant;
public class RedeemRefreshTokenGrantResponse : Response
{
  public RedeemRefreshTokenGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public RedeemRefreshTokenGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string AccessToken { get; init; } = null!;
  public string RefreshToken { get; init; } = null!;
  public string IdToken { get; init; } = null!;
  public int ExpiresIn { get; init; }

}
