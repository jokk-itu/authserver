using MediatR;

namespace Infrastructure.Requests.CreateResource;

#nullable disable
public class CreateResourceCommand : IRequest<CreateResourceResponse>
{
  public string ResourceName { get; set; }

  public ICollection<string> Scopes { get; set; }
}