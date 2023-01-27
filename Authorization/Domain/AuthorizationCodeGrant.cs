using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class AuthorizationCodeGrant
{
  public string Id { get; set; }
  public string Code { get; set; }
  public string Nonce { get; set; }
  public bool IsRedeemed { get; set; }
  public DateTime AuthTime { get; set; }
  public Session Session { get; set; }
  public Client Client { get; set; }

  public static readonly Expression<Func<AuthorizationCodeGrant, bool>> IsValid = a =>
      !a.IsRedeemed && a.AuthTime.AddSeconds(60) > DateTime.UtcNow;
}