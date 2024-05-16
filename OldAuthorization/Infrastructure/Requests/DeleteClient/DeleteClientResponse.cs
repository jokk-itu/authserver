using System.Net;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientResponse : Response
{
  public DeleteClientResponse(HttpStatusCode statusCode) : base(statusCode)
  {
  }

  public DeleteClientResponse(string? errorCode, string? errorDescription, HttpStatusCode statusCode) : base(errorCode, errorDescription, statusCode)
  {
  }
}
