using System.Net;

namespace Infrastructure.Requests.UpdateClient;
public class UpdateClientResponse : Response
{
  public UpdateClientResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public UpdateClientResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode)
    : base(errorCode, errorDescription, statusCode)
  {
  }
}
