namespace Infrastructure.Builders.Token.RefreshToken;
#nullable disable
public class RefreshTokenArguments
{
  public string AuthorizationGrantId { get; init; }
  public string Scope { get; init; }
}