using System.Net;

namespace Application.Validation;
public class ValidationResult
{
  public ValidationResult(HttpStatusCode statusCode)
  {
    StatusCode = statusCode;
  }

  public ValidationResult(string? errorCode, string? errorDescription, HttpStatusCode statusCode)
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

  public static ValidationResult OK()
  {
    return new ValidationResult(HttpStatusCode.OK);
  }
}