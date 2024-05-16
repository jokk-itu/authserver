namespace Infrastructure.Helpers;
public static class TokenHelper
{
  public static bool IsStructuredToken(string token)
  {
    return GetDotLength(token) is 3 or 5;
  }

  private static int GetDotLength(string token)
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      throw new ArgumentException("must not be null or whitespace", token);
    }

    return token.Split('.').Length;
  }
}
