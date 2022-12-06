namespace Infrastructure.Builders.Abstractions;
public interface IFormPostBuilder
{
  string BuildAuthorizationCodeResponse(string redirectUri, string state, string code, string iss);
}