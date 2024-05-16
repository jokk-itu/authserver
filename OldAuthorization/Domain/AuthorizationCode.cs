using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class AuthorizationCode
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public string Value { get; set; }
  public bool IsRedeemed { get; set; }
  public DateTime IssuedAt { get; set; }
  public DateTime? RedeemedAt { get; set; }
  public AuthorizationCodeGrant AuthorizationCodeGrant { get; set; }

  public static readonly Expression<Func<AuthorizationCode, bool>> IsValid = a =>
    !a.IsRedeemed && a.IssuedAt.AddSeconds(30) > DateTime.UtcNow;
}