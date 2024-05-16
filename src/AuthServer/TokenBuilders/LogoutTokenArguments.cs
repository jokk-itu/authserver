namespace AuthServer.TokenBuilders;
public class LogoutTokenArguments
{
    public required string ClientId { get; init; }
    public required string UserId { get; init; }
    public required string SessionId { get; init; }
}