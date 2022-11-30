using System.Net;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantResponse : Response
{
  public RedeemClientCredentialsGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public RedeemClientCredentialsGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string AccessToken { get; init; } = null!;

  public int ExpiresIn { get; init; }
}
