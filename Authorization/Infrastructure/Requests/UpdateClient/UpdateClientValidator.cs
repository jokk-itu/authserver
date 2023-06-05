using Application.Validation;

namespace Infrastructure.Requests.UpdateClient;
public class UpdateClientValidator : IValidator<UpdateClientCommand>
{
  public Task<ValidationResult> ValidateAsync(UpdateClientCommand value, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
