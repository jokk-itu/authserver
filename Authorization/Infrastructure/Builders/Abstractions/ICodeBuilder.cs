namespace Infrastructure.Builders.Abstractions;
public interface ICodeBuilder
{
  Task<string> BuildAuthorizationCodeAsync(
    string authorizationGrantId,
    string codeChallenge, 
    string codeChallengeMethod,
    ICollection<string> scopes);

  Task<string> BuildLoginCodeAsync(string userId);
}