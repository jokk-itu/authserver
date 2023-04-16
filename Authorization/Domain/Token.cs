using System.Security.Cryptography;
using Domain.Enums;

namespace Domain;

public abstract class Token
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Reference { get; set; } = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
  public string Scope { get; set; } = null!; 
  public TokenType TokenType { get; set; }
  public DateTime ExpiresAt { get; set; }
  public DateTime IssuedAt { get; set; }
  public DateTime NotBefore { get; set; }
  public string Audience { get; set; } = null!;
  public string Issuer { get; set; } = null!;
  public DateTime? RevokedAt { get; set; }
}