using System.Net;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.DeleteClient;
public class DeleteClientHandler : IRequestHandler<DeleteClientCommand, DeleteClientResponse>
{
  private readonly IdentityContext _identityContext;

  public DeleteClientHandler(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<DeleteClientResponse> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    _identityContext.Remove(client);
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new DeleteClientResponse(HttpStatusCode.NoContent);
  }
}