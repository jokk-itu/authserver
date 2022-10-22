using Domain.Constants;
using MediatR;

namespace Infrastructure.Requests.CreateClient;

#nullable disable
public class CreateClientCommand : IRequest<CreateClientResponse>
{
  public string ClientName { get; set; }
  public ICollection<string> RedirectUris { get; set; }
  public ICollection<string> ResponseTypes { get; set; } = new[] { ResponseTypeConstants.Code };
  public ICollection<string> GrantTypes { get; set; }
  public string ApplicationType { get; set; }
  public ICollection<string> Contacts { get; set; }
  public ICollection<string> Scopes { get; set; }
  public string PolicyUri { get; set; }
  public string TosUri { get; set; }
  public string SubjectType { get; set; } = SubjectTypeConstants.Public;
  public string TokenEndpointAuthMethod { get; set; } = TokenEndpointAuthMethodConstants.ClientSecretPost;
}