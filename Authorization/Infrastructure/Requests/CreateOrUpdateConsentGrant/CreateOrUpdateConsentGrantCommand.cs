using MediatR;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;

#nullable disable
public class CreateOrUpdateConsentGrantCommand : IRequest<CreateOrUpdateConsentGrantResponse>
{
  public string ClientId { get; init; }
  public string UserId { get; init; }
  public ICollection<string> ConsentedScopes { get; init; }
  public ICollection<string> ConsentedClaims { get; init; }
}
