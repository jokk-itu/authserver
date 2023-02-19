using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class Session
{
  public int Id { get; set; }
  public bool IsRevoked { get; set; }
  public User User { get; set; }
  public ICollection<AuthorizationCodeGrant> AuthorizationCodeGrants { get; set; } = new List<AuthorizationCodeGrant>();
}