namespace Infrastructure.Builders.Token.LogoutToken;
#nullable disable
public class LogoutTokenArguments
{
  public string ClientId { get; init; }
  public string UserId { get; init; }
  public string SessionId { get; init; }
}