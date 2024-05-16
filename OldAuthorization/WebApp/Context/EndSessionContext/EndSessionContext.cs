namespace WebApp.Context.EndSessionContext;

#nullable disable
public class EndSessionContext
{
  public string IdTokenHint { get; set; }
  public string ClientId { get; set; }
  public string PostLogoutRedirectUri { get; set; }
  public string State { get; set; }
}