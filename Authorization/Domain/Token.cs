using Domain.Enums;

namespace Domain;

#nullable disable
public abstract class Token
{
  public long Id { get; set; }
  public DateTime Created { get; set; }
  public bool IsRevoked { get; set; }
  public string Value { get; set; }
  public TokenType TokenType { get; set; }
}