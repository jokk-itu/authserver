using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantHandler : IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateAuthorizationGrantCommand> _validator;
  private readonly ICodeBuilder _codeBuilder;
  private readonly ICodeDecoder _codeDecoder;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext, 
    IValidator<CreateAuthorizationGrantCommand> validator,
    ICodeBuilder codeBuilder,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _codeBuilder = codeBuilder;
    _codeDecoder = codeDecoder;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
    {
      return new CreateAuthorizationGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

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
    var authTime = DateTime.UtcNow;

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      grantId,
      request.CodeChallenge,
      request.CodeChallengeMethod,
      request.Scope.Split(' '));

    var maxAge = string.IsNullOrWhiteSpace(request.MaxAge) ? client.DefaultMaxAge : long.Parse(request.MaxAge);
    var authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = grantId,
      IsCodeRedeemed = false,
      Client = client,
      Code = code,
      AuthTime = authTime,
      MaxAge = maxAge,
      Nonce = request.Nonce,
      Session = session
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