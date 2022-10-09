namespace Domain;

#nullable disable
public class Claim
{
  public int Id { get; set; }
  public string Name { get; set; }
  public ICollection<ConsentGrant> ConsentGrants { get; set; }
}