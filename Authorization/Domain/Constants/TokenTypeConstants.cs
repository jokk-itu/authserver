namespace Domain.Constants;
public static class TokenTypeConstants
{
  public const string RefreshToken = "refresh_token";

  /// <summary>
  /// A stored access_token is pointed to by a reference token
  /// </summary>
  public const string AccessToken = "access_token";
}