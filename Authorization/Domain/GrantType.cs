namespace Domain;

#nullable disable
public class GrantType
{
  public int Id { get; set; }

  public string Name { get; set; }

  public ICollection<Client> Clients { get; set; }
}