namespace Infrastructure.Builders.Abstractions;
public interface ITokenBuilder
{
    Task<string> BuildAccessToken(string clientId, ICollection<string> scopes, string userId, string sessionId, CancellationToken cancellationToken = default);
    Task<string> BuildClientAccessToken(string clientId, ICollection<string> scopes, CancellationToken cancellationToken = default);
    Task<string> BuildRefreshToken(string authorizationGrantId, string clientId, ICollection<string> scopes, string userId, string sessionId, CancellationToken cancellationToken = default);
    Task<string> BuildIdToken(string authorizationGrantId, string clientId, ICollection<string> scopes, string nonce, string userId, string sessionId, DateTime authTime, CancellationToken cancellationToken = default);
    string BuildLogoutToken(string clientId, string sessionId, string userId, CancellationToken cancellationToken = default);
    string BuildResourceInitialAccessToken();
    string BuildClientInitialAccessToken();
    string BuildClientRegistrationAccessToken(string clientId);
    string BuildResourceRegistrationAccessToken(string resourceId); 
    string BuildScopeInitialAccessToken();
    string BuildScopeRegistrationAccessToken(string scopeId);
}
