using MediatR;

namespace Infrastructure.Requests.HasUserConsent;

#nullable disable
public class HasUserConsentQuery : IRequest<HasUserConsentResponse>
{
  public string Username { get; set; }

  public string Password { get; set; }

  public string ClientId { get; set; }

  public ICollection<string> Scopes { get; set; }
}