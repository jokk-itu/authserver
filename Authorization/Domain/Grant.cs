namespace Domain;

#nullable disable
public class Grant
{
  public int Id { get; set; }

  public string Name { get; set; }

  public ICollection<Client> Clients { get; set; }
}