using MediatR;

namespace Infrastructure.Requests.UpdateClient;
public class UpdateClientHandler : IRequestHandler<UpdateClientCommand, UpdateClientResponse>
{
  public Task<UpdateClientResponse> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}