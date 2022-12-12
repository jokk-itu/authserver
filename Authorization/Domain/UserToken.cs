using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class UserToken
{
  public long Id { get; set; }
  public string Value { get; set; }
  public bool IsRedeemed { get; set; }
  public DateTime ExpiresAt { get; set; }
  public User User { get; set; }

  public static readonly Expression<Func<UserToken, bool>> IsActive = u =>
    DateTime.UtcNow < u.ExpiresAt && !u.IsRedeemed;
}