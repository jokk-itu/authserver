namespace OAuthService;

public class PasswordRehashFailedException : Exception
{
  public PasswordRehashFailedException() { }

  public PasswordRehashFailedException(string message) : base(message) { }

  public PasswordRehashFailedException(string message, Exception innerException) : base(message, innerException) { }
}