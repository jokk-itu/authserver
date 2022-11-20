namespace Application.Validation;
public interface IBaseValidator<in T>
{
  public Task<BaseValidationResult> ValidateAsync(T value, CancellationToken cancellationToken = default);
}