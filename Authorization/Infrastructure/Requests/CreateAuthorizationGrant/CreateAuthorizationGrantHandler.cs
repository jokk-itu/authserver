using System.Net;
using Domain;
using Infrastructure.Builders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantHandler : IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeBuilder _codeBuilder;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext,
    ICodeBuilder codeBuilder)
  {
    _identityContext = identityContext;
    _codeBuilder = codeBuilder;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var user = await _identityContext.Set<User>().SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<Session>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => !x.IsRevoked)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken) ?? new Session
      {
        User = user,
      };

    var grantId = Guid.NewGuid().ToString();
    var codeId = Guid.NewGuid().ToString();
    var nonceId = Guid.NewGuid().ToString();
    var authTime = DateTime.UtcNow;

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      grantId,
      codeId,
      nonceId,
      request.CodeChallenge,
      request.CodeChallengeMethod,
      request.Scope.Split(' '));

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
      Value = request.Nonce
    };

    var maxAge = string.IsNullOrWhiteSpace(request.MaxAge) ? client.DefaultMaxAge : long.Parse(request.MaxAge);
    var authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = grantId,
      Client = client,
      AuthTime = authTime,
      MaxAge = maxAge,
      Session = session,
      AuthorizationCodes = new[] { authorizationCode },
      Nonces = new [] { nonce }
    };

    await _identityContext
      .Set<AuthorizationCodeGrant>()
      .AddAsync(authorizationCodeGrant, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateAuthorizationGrantResponse(HttpStatusCode.OK)
    {
      Code = code,
      State = request.State
    };
  }
}