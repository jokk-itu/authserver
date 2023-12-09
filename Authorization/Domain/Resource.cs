namespace Domain;

#nullable disable
public class Resource
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; }
  public string Uri { get; set; }
  public string Secret { get; set; }
  public ICollection<Scope> Scopes { get; set; } = new List<Scope>();
}