namespace Application.Validation;
public abstract class BaseValidator<T>
{
  protected readonly bool IsRequired;

  protected BaseValidator(bool isRequired)
  {
    IsRequired = isRequired;
  }
  public abstract Task<BaseValidationResult> IsValidAsync(T value);

  protected Task<BaseValidationResult> GetValidResult()
  {
    return new Task<BaseValidationResult>(() => new BaseValidationResult());
  }

  protected Task<BaseValidationResult> GetInvalidResult(string error, string errorDescription)
  {
    return new Task<BaseValidationResult>(() => new BaseValidationResult(error, errorDescription));
  }
}
