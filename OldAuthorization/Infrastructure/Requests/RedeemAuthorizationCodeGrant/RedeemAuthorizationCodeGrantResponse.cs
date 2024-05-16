using System.Net;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;
public class RedeemAuthorizationCodeGrantResponse : Response
{
  public RedeemAuthorizationCodeGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public RedeemAuthorizationCodeGrantResponse(string errorCode, string errorDescription, HttpStatusCode statusCode)
    : base(errorCode, errorDescription, statusCode)
  {
  }

  public string AccessToken { get; init; } = null!;
  public string? RefreshToken { get; init; }
  public string IdToken { get; init; } = null!;
  public int ExpiresIn { get; init; }
  public string Scope { get; init; } = null!;
}