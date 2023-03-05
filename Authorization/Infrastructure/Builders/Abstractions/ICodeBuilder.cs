namespace Infrastructure.Builders.Abstractions;
public interface ICodeBuilder
{
  Task<string> BuildAuthorizationCodeAsync(
    string authorizationGrantId,
    string authorizationCodeId,
    string nonceId,
    string codeChallenge, 
    string codeChallengeMethod,
    ICollection<string> scopes);
}