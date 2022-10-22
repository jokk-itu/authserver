using MediatR;

namespace Infrastructure.Requests.DeleteClient;

#nullable disable
public class DeleteClientCommand : IRequest<DeleteClientResponse>
{
  public string ClientRegistrationToken { get; init; }
}
