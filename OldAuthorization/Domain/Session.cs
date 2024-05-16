namespace Domain;

#nullable disable
public class Session
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public bool IsRevoked { get; set; }
  public User User { get; set; }
  public ICollection<AuthorizationCodeGrant> AuthorizationCodeGrants { get; set; } = new List<AuthorizationCodeGrant>();
}