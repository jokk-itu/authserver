using Infrastructure.Requests.Abstract;

namespace WebApp.Context.RevocationContext;

#nullable disable
public class RevocationContext
{
  public string Token { get; set; }
  public string TokenTypeHint { get; set; }
  public ICollection<ClientAuthentication> ClientAuthentications { get; set; } = new List<ClientAuthentication>();
}