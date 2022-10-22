namespace Application.Validation;
public abstract class BaseValidator<T>
{
  public abstract Task<BaseValidationResult> ValidateAsync(T value, CancellationToken cancellationToken = default);

  protected Task<BaseValidationResult> GetValidResult()
  {
    return new Task<BaseValidationResult>(() => new BaseValidationResult());
  }

  protected Task<BaseValidationResult> GetInvalidResult(string error, string errorDescription)
  {
    return new Task<BaseValidationResult>(() => new BaseValidationResult(error, errorDescription));
  }

  protected BaseValidationResult Ok()
  {
    return new BaseValidationResult();
  }
}
