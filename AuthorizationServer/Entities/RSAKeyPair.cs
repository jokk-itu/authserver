namespace AuthorizationServer.Entities;

public class RSAKeyPair
{
  public string Purpose { get; init; }

  public string PublicKey { get; init; }

  public string PrivateKey { get; init; }

  public DateTime NotBefore { get; init; }

  public DateTime Expiration { get; init; }

  public bool IsRevoked { get; init; }
}