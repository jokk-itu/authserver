namespace Infrastructure.Builders.Abstractions;
public interface ICodeBuilder
{
  Task<string> BuildAuthorizationCodeAsync(
    long authorizationGrantId,
    string codeChallenge, 
    string codeChallengeMethod,
    string userId,
    string clientId,
    ICollection<string> scopes);
}