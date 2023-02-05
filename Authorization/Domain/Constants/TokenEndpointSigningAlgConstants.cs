namespace Domain.Constants;
public static class TokenEndpointSigningAlgConstants
{
  public const string Rsa = "RS256";

  public static readonly string[] TokenEndpointSigningAlgorithms = { Rsa, };
}