using Domain.Enums;

namespace Infrastructure.Requests.Abstract;

#nullable disable
public class ClientAuthentication
{
  public TokenEndpointAuthMethod Method { get; set; }
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
  public string ClientAssertion { get; set; }
  public string ClientAssertionType { get; set; }
}