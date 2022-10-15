namespace Domain;

#nullable disable
public class AuthorizationCodeGrant
{
  public string Id { get; set; }
  public string Code { get; set; }
  public string Nonce { get; set; }
  public bool IsRedeemed { get; set; }
  public Session Session { get; set; }
  public Client Client { get; set; }
}