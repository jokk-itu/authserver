namespace Application.Validation;
public interface IValidator<in T>
{
  Task<ValidationResult> ValidateAsync(T value, CancellationToken cancellationToken = default);
}