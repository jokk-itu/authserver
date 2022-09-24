using Application.Validation;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientValidator : IValidator<DeleteClientCommand>
{
  private readonly IdentityContext _identityContext;

  public DeleteClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public Task<ValidationResult> IsValidAsync(DeleteClientCommand value)
  {
    // TODO no empty accesstoken
    // TODO make sure client_id exists in token
    // TODO make sure client_id exists in database
    throw new NotImplementedException();
  }
}
