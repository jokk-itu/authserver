namespace Domain;

public class PairwiseIdentifier
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public User User { get; set; } = null!;
  public Client Client { get; set; } = null!;
}
