using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class AuthorizationCodeGrant
{
  public string Id { get; set; }
  public string Code { get; set; }
  public string Nonce { get; set; }
  public bool IsCodeRedeemed { get; set; }
  public DateTime AuthTime { get; set; }
  public long? MaxAge { get; set; }
  public bool IsRevoked { get; set; }
  public Session Session { get; set; }
  public Client Client { get; set; }

  public static readonly Expression<Func<AuthorizationCodeGrant, bool>> IsCodeValid = a =>
    !a.IsCodeRedeemed && !a.IsRevoked && a.AuthTime.AddSeconds(30) > DateTime.UtcNow;

  public static readonly Expression<Func<AuthorizationCodeGrant, bool>> IsMaxAgeValid = a =>
    !a.IsRevoked && (a.MaxAge == null || a.AuthTime.AddSeconds(a.MaxAge.Value) > DateTime.UtcNow);
}