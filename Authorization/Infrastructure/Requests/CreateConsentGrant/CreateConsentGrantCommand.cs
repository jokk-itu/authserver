using MediatR;

namespace Infrastructure.Requests.CreateConsentGrant;

#nullable disable
public class CreateConsentGrantCommand : IRequest<CreateConsentGrantResponse>
{
  public string Username { get; set; }
  public string Password { get; set; }
  public long MaxAge { get; set; }
  public string ClientId { get; set; }
  public ICollection<string> ConsentedClaims { get; set; }
  public ICollection<string> Scopes { get; set; }
}