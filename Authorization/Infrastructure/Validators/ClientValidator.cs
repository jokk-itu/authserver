using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators;
public class ClientValidator : BaseValidator<ClientToValidate>
{
  private readonly IdentityContext _identityContext;

  public ClientValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public override async Task<BaseValidationResult> ValidateAsync(ClientToValidate value, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(value.ClientId))
      return new BaseValidationResult(ErrorCode.InvalidClient, "client_id is invalid");

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == value.ClientId, cancellationToken: cancellationToken);

    if (client is null)
      return new BaseValidationResult(ErrorCode.InvalidClient, "client_id is invalid");

    if(!string.IsNullOrWhiteSpace(value.ClientSecret) && client.Secret != value.ClientSecret)
      return new BaseValidationResult(ErrorCode.InvalidClient, "client_secret is invalid");

    return Ok();
  }
}

public record ClientToValidate(string ClientId, string? ClientSecret);