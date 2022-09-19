namespace Infrastructure.Builders.Abstractions;
public interface ITokenBuilder
{
    Task<string> BuildAccessTokenAsync(string clientId, ICollection<string> scopes, string userId, CancellationToken cancellationToken = default);
    Task<string> BuildRefreshTokenAsync(string clientId, ICollection<string> scopes, string userId, CancellationToken cancellationToken = default);
    string BuildIdToken(string clientId, ICollection<string> scopes, string nonce, string userId);

    string BuildResourceInitialAccessToken();
    string BuildClientInitialAccessToken();
    string BuildClientRegistrationAccessToken(string clientId);
    string BuildResourceRegistrationAccessToken(string resourceId);
}
