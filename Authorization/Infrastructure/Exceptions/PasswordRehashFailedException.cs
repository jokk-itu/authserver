using System.Runtime.Serialization;

namespace AuthorizationServer.Exceptions;

[Serializable]
public class PasswordRehashFailedException : Exception
{
  public PasswordRehashFailedException() { }

  public PasswordRehashFailedException(string message) : base(message) { }

  public PasswordRehashFailedException(string message, Exception innerException) : base(message, innerException) { }

  protected PasswordRehashFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}