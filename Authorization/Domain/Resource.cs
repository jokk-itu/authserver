namespace Domain;

#nullable disable
public class Resource
{
  public int Id { get; set; }

  public string Name { get; set; }

  public string Secret { get; set; }

  public ICollection<Scope> Scopes { get; set; }
}