namespace Domain;

#nullable disable
public class AccessToken : Token
{
  public Session Session { get; set; }
  public Client Client { get; set; }
}