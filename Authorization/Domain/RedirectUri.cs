namespace Domain;

#nullable disable
public class RedirectUri
{
  public int Id { get; set; }
  public string Uri { get; set; }
  public Client Client { get; set; }
}