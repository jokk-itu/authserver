using MediatR;

namespace Infrastructure.Requests.CreateConsentGrant;

#nullable disable
public class CreateConsentGrantCommand : IRequest<CreateConsentGrantResponse>
{
  public string ClientId { get; init; }
  public string UserId { get; init; }
  public ICollection<string> ConsentedScopes { get; init; }
  public ICollection<string> ConsentedClaims { get; init; }
}