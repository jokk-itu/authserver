using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Domain;

#nullable disable
public class AuthorizationCodeGrant
{
  public string Id { get; set; }
  public DateTime AuthTime { get; set; }
  public long? MaxAge { get; set; }
  public bool IsRevoked { get; set; }
  public Session Session { get; set; }
  public Client Client { get; set; }
  public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new List<AuthorizationCode>();
  public ICollection<Nonce> Nonces { get; set; } = new List<Nonce>();
  public ICollection<GrantToken> GrantTokens { get; set; } = new List<GrantToken>();

  public static Expression<Func<AuthorizationCodeGrant, bool>> IsAuthorizationCodeValid(string authorizationCodeId) =>
    a => a.AuthorizationCodes.Any()
         && a.AuthorizationCodes.AsQueryable().Where(AuthorizationCode.IsValid).Any(x => x.Id == authorizationCodeId)
         && !a.IsRevoked
         && a.AuthTime.AddSeconds(30) > DateTime.UtcNow;

  public static readonly Expression<Func<AuthorizationCodeGrant, bool>> IsMaxAgeValid = a =>
    !a.IsRevoked && (a.MaxAge == null || a.AuthTime.AddSeconds(a.MaxAge.Value) > DateTime.UtcNow);
}