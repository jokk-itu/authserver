namespace AuthorizationServer.Entities;

public record AsymmetricKeyPair
{
  public string Purpose { get; init; }
  public byte[] PublicKey { get; init; }
  
  public byte[] PrivateKey { get; init; }

  public DateTimeOffset NotBefore { get; init; }

  public DateTimeOffset Expiration { get; init; }

  public bool IsRevoked { get; init; }
}