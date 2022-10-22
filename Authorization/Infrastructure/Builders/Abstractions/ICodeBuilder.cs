namespace Infrastructure.Builders.Abstractions;
public interface ICodeBuilder
{
  Task<string> BuildAuthorizationCodeAsync(
    string authorizationGrantId,
    string codeChallenge, 
    string codeChallengeMethod,
    string userId,
    string clientId,
    ICollection<string> scopes);
}