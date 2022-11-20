using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientValidator : IValidator<DeleteClientCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public DeleteClientValidator(IdentityContext identityContext, ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(DeleteClientCommand value, CancellationToken cancellationToken = default)
  {
    if (await IsClientRegistrationTokenInvalid(value))
      return new ValidationResult(ErrorCode.InvalidClientMetadata, "token is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsClientRegistrationTokenInvalid(DeleteClientCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ClientRegistrationToken))
      return true;

    var clientId = _tokenDecoder.DecodeSignedToken(command.ClientRegistrationToken)?.Claims?
      .SingleOrDefault(x => x.Type == ClaimNameConstants.ClientId)?.Value;
    if (clientId is null)
      return true;
    if (!await _identityContext.Set<Client>().AnyAsync(x => x.Id == clientId))
      return true;

    return false;
  }
}
