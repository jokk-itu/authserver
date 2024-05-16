using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Services;

public class AuthorizationGrantService : IAuthorizationGrantService
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeBuilder _codeBuilder;

  public AuthorizationGrantService(
    IdentityContext identityContext,
    ICodeBuilder codeBuilder)
  {
    _identityContext = identityContext;
    _codeBuilder = codeBuilder;
  }

  public async Task<AuthorizationGrantResult> CreateAuthorizationGrant(CreateAuthorizationGrantArguments arguments,
    CancellationToken cancellationToken)
  {
    var grantId = Guid.NewGuid().ToString();
    var codeId = Guid.NewGuid().ToString();
    var nonceId = Guid.NewGuid().ToString();
    var authTime = DateTime.UtcNow;

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      grantId,
      codeId,
      nonceId,
      arguments.CodeChallenge,
      arguments.CodeChallengeMethod,
      arguments.Scope.Split(' '),
      arguments.RedirectUri);

    var authorizationCode = new AuthorizationCode
    {
      Id = codeId,
      IsRedeemed = false,
      IssuedAt = DateTime.UtcNow,
      Value = code
    };

    var nonce = new Nonce
    {
      Id = nonceId,
      IssuedAt = DateTime.UtcNow,
      Value = arguments.Nonce
    };

    var authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = grantId,
      Client = arguments.Client,
      AuthTime = authTime,
      MaxAge = arguments.MaxAge,
      Session = arguments.Session,
      AuthorizationCodes = new[] { authorizationCode },
      Nonces = new[] { nonce }
    };

    await _identityContext
      .Set<AuthorizationCodeGrant>()
      .AddAsync(authorizationCodeGrant, cancellationToken: cancellationToken);

    return new AuthorizationGrantResult(code);
  }

  public async Task<AuthorizationGrantResult> UpdateAuthorizationGrant(UpdateAuthorizationGrantArguments arguments,
    CancellationToken cancellationToken)
  {
    var authorizationCodeId = Guid.NewGuid().ToString();
    var nonceId = Guid.NewGuid().ToString();
    var scopes = arguments.Scope.Split(' ');

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      arguments.AuthorizationCodeGrant.Id,
      authorizationCodeId,
      nonceId,
      arguments.CodeChallenge,
      arguments.CodeChallengeMethod,
      scopes,
      arguments.RedirectUri);

    var authorizationCode = new AuthorizationCode
    {
      Id = authorizationCodeId,
      IssuedAt = DateTime.UtcNow,
      Value = code
    };

    var nonce = new Nonce
    {
      Id = nonceId,
      IssuedAt = DateTime.UtcNow,
      Value = arguments.Nonce
    };

    arguments.AuthorizationCodeGrant.AuthorizationCodes.Add(authorizationCode);
    arguments.AuthorizationCodeGrant.Nonces.Add(nonce);

    return new AuthorizationGrantResult(code);
  }
}

public record AuthorizationGrantResult(string Code);

public class CreateAuthorizationGrantArguments
{
  public Session Session { get; init; } = null!;
  public Client Client { get; init; } = null!;
  public string CodeChallenge { get; init; } = null!;
  public string CodeChallengeMethod { get; init; } = null!;
  public string? RedirectUri { get; init; }
  public string Nonce { get; init; } = null!;
  public long? MaxAge { get; init; }
  public string Scope { get; init; } = null!;
}

public class UpdateAuthorizationGrantArguments
{
  public AuthorizationCodeGrant AuthorizationCodeGrant { get; init; } = null!;
  public string CodeChallenge { get; init; } = null!;
  public string CodeChallengeMethod { get; init; } = null!;
  public string Nonce { get; init; } = null!;
  public string Scope { get; init; } = null!;
  public string? RedirectUri { get; init; }
}