namespace Application.Validation;

public class BaseValidationResult
{
  public BaseValidationResult()
  {
  }

  public BaseValidationResult(string errorCode, string errorDescription)
  {
    ErrorCode = errorCode;
    ErrorDescription = errorDescription;
  }

  public string? ErrorCode { get; }

  public string? ErrorDescription { get; }

  public bool IsError()
  {
    return !string.IsNullOrWhiteSpace(ErrorCode);
  }
}