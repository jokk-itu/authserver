using MediatR;

namespace Infrastructure.Requests.GetConsentModel;

#nullable disable
public class GetConsentModelQuery : IRequest<GetConsentModelResponse>
{
  public string ClientId { get; init; }
  public string UserId { get; init; }
  public string Scope { get; init; }
}