namespace OAuthService.Entities
{
  public class RSAKeyPair
  {
    public string Purpose { get; set; }

    public string PublicKey { get; set; }

    public string PrivateKey { get; set; }

    public DateTime NotBefore { get; set; }

    public DateTime Expiration { get; set; }

    public bool IsRevoked { get; set; }
  }
}