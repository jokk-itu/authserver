using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientValidator : IValidator<DeleteClientCommand>
{
  private readonly IdentityContext _identityContext;

  public DeleteClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(DeleteClientCommand value, CancellationToken cancellationToken = default)
  {
    var isAuthenticated = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == value.ClientId)
      .SelectMany(x => x.ClientTokens)
      .OfType<RegistrationToken>()
      .Where(x => x.Reference == value.Token)
      .Where(x => x.RevokedAt == null)
      .AnyAsync(cancellationToken: cancellationToken);

    if (!isAuthenticated)
    {
      // TODO revoke
      return new ValidationResult(ErrorCode.InvalidClientMetadata, "request is invalid", HttpStatusCode.Unauthorized);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}