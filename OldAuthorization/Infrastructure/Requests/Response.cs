using System.Net;

namespace Infrastructure.Requests;

public abstract class Response
{
  protected Response(HttpStatusCode statusCode)
  {
    StatusCode = statusCode;
  }

  protected Response(string errorCode, string errorDescription, HttpStatusCode statusCode)
  {
    ErrorCode = errorCode;
    ErrorDescription = errorDescription;
    StatusCode = statusCode;
  }

  public string? ErrorCode { get; init; }
  public string? ErrorDescription { get; init; }
  public HttpStatusCode StatusCode { get; init; }

  public bool IsError()
  {
    return !string.IsNullOrWhiteSpace(ErrorCode);
  }
}