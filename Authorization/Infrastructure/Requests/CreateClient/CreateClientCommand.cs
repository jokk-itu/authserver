using Domain.Constants;
using MediatR;

namespace Infrastructure.Requests.CreateClient;

public class CreateClientCommand : IRequest<CreateClientResponse>
{
  public string ClientName { get; set; } = null!;
  public ICollection<string> RedirectUris { get; set; } = new List<string>();
  public ICollection<string> PostLogoutRedirectUris { get; set; } = new List<string>();
  public ICollection<string> ResponseTypes { get; set; } = new[] { ResponseTypeConstants.Code };
  public ICollection<string> GrantTypes { get; set; } = new List<string>();
  public string ApplicationType { get; set; } = null!;
  public ICollection<string> Contacts { get; set; } = new List<string>();
  public string Scope { get; set; } = null!;
  public string? PolicyUri { get; set; }
  public string? TosUri { get; set; }
  public string SubjectType { get; set; } = SubjectTypeConstants.Public;
  public string TokenEndpointAuthMethod { get; set; } = TokenEndpointAuthMethodConstants.ClientSecretPost;
  public string? DefaultMaxAge { get; set; }
  public string? InitiateLoginUri { get; set; }
  public string? LogoUri { get; set; }
  public string? ClientUri { get; set; }
  public string? BackChannelLogoutUri { get; set; }
}