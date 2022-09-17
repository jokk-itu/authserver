namespace Application.Validation;
public interface IValidator<in T>
{
  Task<ValidationResult> IsValidAsync(T value);
}