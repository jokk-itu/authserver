using System.Net;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantResponse : Response
{
  public CreateAuthorizationGrantResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public CreateAuthorizationGrantResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }

  public string State { get; init; } = null!;

  public string Code { get; init; } = null!;
}
