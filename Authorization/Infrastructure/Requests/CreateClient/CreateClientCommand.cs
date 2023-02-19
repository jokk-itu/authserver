using Domain.Constants;
using MediatR;

namespace Infrastructure.Requests.CreateClient;

public class CreateClientCommand : IRequest<CreateClientResponse>
{
  public string ClientName { get; set; } = null!;
  public ICollection<string> RedirectUris { get; set; } = new List<string>();
  public ICollection<string> ResponseTypes { get; set; } = new[] { ResponseTypeConstants.Code };
  public ICollection<string> GrantTypes { get; set; } = new List<string>();
  public string ApplicationType { get; set; } = null!;
  public ICollection<string> Contacts { get; set; } = new List<string>();
  public string Scope { get; set; } = null!;
  public string? PolicyUri { get; set; }
  public string? TosUri { get; set; }
  public string SubjectType { get; set; } = SubjectTypeConstants.Public;
  public string TokenEndpointAuthMethod { get; set; } = TokenEndpointAuthMethodConstants.ClientSecretPost;
  public string? DefaultMaxAge { get; init; }
  public string? InitiateLoginUri { get; init; }
  public string? LogoUri { get; init; }
  public string? ClientUri { get; init; }
}