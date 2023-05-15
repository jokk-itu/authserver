namespace WebApp.Context.RevocationContext;

#nullable disable
public class RevocationContext
{
  public string Token { get; set; }
  public string TokenTypeHint { get; set; }
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
}