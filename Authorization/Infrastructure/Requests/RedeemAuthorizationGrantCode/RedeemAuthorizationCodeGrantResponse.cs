using System.Net;

namespace Infrastructure.Requests.CreateAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantResponse : Response
{
  public RedeemAuthorizationCodeGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public RedeemAuthorizationCodeGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string AccessToken { get; init; } = null!;
  public string RefreshToken { get; init; } = null!;
  public string IdToken { get; init; } = null!;
  public int ExpiresIn { get; init; }
}
