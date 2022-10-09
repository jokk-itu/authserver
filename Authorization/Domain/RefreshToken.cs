namespace Domain;

#nullable disable
public class RefreshToken : Token
{
  public Session Session { get; set; }
  public Client Client { get; set; }
  public User User { get; set; }
}