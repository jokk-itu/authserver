namespace Domain;

public class Token
{
  public long KeyId { get; set; }

  public TokenType TokenType { get; set; } = null!;

  public string Value { get; set; } = null!;

  public DateTime? RevokedAt { get; set; }

  public User? RevokedBy { get; set; }
}