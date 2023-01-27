namespace Domain;

#nullable disable
public class Scope
{
  public int Id { get; set; }
  public string Name { get; set; }
  public ICollection<Resource> Resources { get; set; } = new List<Resource>();
  public ICollection<Client> Clients { get; set; } = new List<Client>();
  public ICollection<ConsentGrant> ConsentGrants { get; set; } = new List<ConsentGrant>();
}