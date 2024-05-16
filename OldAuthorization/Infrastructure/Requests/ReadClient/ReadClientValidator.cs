using Application;
using Application.Validation;
using Domain;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.ReadClient;
public class ReadClientValidator : IValidator<ReadClientQuery>
{
  private readonly IdentityContext _identityContext;

  public ReadClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(ReadClientQuery value, CancellationToken cancellationToken = default)
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
