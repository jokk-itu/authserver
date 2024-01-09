using Infrastructure.Requests.Abstract;

namespace WebApp.Context.IntrospectionContext;

#nullable disable
public class IntrospectionContext
{
  public string Token { get; set; }
  public string TokenTypeHint { get; set; }
  public ICollection<ClientAuthentication> ClientAuthentications { get; set; } = new List<ClientAuthentication>();
}