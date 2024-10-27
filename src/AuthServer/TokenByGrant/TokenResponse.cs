namespace AuthServer.TokenByGrant;

internal class TokenResponse
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public string? IdToken { get; init; }
    public required long ExpiresIn { get; init; }
    public required string Scope { get; init; }
}