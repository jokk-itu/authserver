namespace Domain;

#nullable disable
public class Resource
{
  public int Id { get; set; }

  public string Name { get; set; }

  public string SecretHash { get; set; }

  public ICollection<Scope> Scopes { get; set; }
}