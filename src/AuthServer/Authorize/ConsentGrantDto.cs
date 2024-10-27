namespace AuthServer.Authorize;

public class ConsentGrantDto
{
    public required string Username { get; init; }
    public required string ClientName { get; init; }
    public string? ClientUri { get; init; }
    public string? ClientLogoUri { get; init; }
    public required IEnumerable<string> ConsentedScope { get; init; } = [];
    public required IEnumerable<string> ConsentedClaims { get; init; } = [];
}