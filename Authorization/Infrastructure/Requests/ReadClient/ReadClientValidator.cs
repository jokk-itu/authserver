using Application.Validation;

namespace Infrastructure.Requests.ReadClient;
public class ReadClientValidator : IValidator<ReadClientQuery>
{
  private readonly IdentityContext _identityContext;

  public ReadClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public Task<ValidationResult> ValidateAsync(ReadClientQuery value, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
