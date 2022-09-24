using System.Net;
using System.Security.Cryptography.X509Certificates;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, DeleteClientResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<DeleteClientCommand> _validator;
  private readonly ITokenDecoder _tokenDecoder;

  public DeleteClientHandler(
    IdentityContext identityContext,
    IValidator<DeleteClientCommand> validator,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<DeleteClientResponse> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.IsValidAsync(request);
    if (validationResult.IsError())
      return new DeleteClientResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var clientId = _tokenDecoder.DecodeToken(request.ClientRegistrationToken)!.Claims.Single(x => x.Type == ClaimNameConstants.ClientId).Value;
    _identityContext.Set<Client>().Remove(new Client { Id = clientId});
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new DeleteClientResponse(HttpStatusCode.NoContent);
  }
}
