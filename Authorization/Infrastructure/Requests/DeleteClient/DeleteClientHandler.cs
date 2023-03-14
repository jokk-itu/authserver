using System.Net;
using Application;
using Domain;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, DeleteClientResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ITokenDecoder _tokenDecoder;

  public DeleteClientHandler(
    IdentityContext identityContext,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<DeleteClientResponse> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
  {
    var clientId = _tokenDecoder.DecodeSignedToken(request.ClientRegistrationToken)!.Claims
      .Single(x => x.Type == ClaimNameConstants.ClientId).Value;

    var client = await _identityContext.Set<Client>().FindAsync(new object?[] { clientId }, cancellationToken: cancellationToken);
    if (client is null)
      return new DeleteClientResponse(ErrorCode.InvalidClientMetadata, "client_id does not exist", HttpStatusCode.BadRequest);

    _identityContext.Set<Client>().Remove(client);
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new DeleteClientResponse(HttpStatusCode.NoContent);
  }
}