using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantHandler : IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateAuthorizationGrantCommand> _validator;
  private readonly UserManager<User> _userManager;
  private readonly ICodeBuilder _codeBuilder;
  private readonly ICodeDecoder _codeDecoder;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext, 
    IValidator<CreateAuthorizationGrantCommand> validator,
    UserManager<User> userManager,
    ICodeBuilder codeBuilder,
    ICodeDecoder codeDecoder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _userManager = userManager;
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

    var loginCode = _codeDecoder.DecodeLoginCode(request.LoginCode);
    var userId = loginCode.UserId;

    var user = await _userManager.FindByIdAsync(userId);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<Session>()
      .Include(x => x.Clients)
      .SingleOrDefaultAsync(x => x.User.Id == userId, cancellationToken: cancellationToken) ?? new Session
      {
        Created = DateTime.Now,
        Updated = DateTime.Now,
        User = user,
        MaxAge = request.MaxAge,
        Clients = new[] { client }
      };

    if (session.Clients.All(x => x.Id != request.ClientId))
    {
      session.Clients.Add(client);
    }

    var grantId = Guid.NewGuid().ToString();
    var authTime = DateTime.UtcNow;

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      grantId,
      request.CodeChallenge,
      request.CodeChallengeMethod,
      user.Id,
      client.Id,
      request.Scopes);

    var authorizationCodeGrant = new AuthorizationCodeGrant
    {
      Id = grantId,
      IsRedeemed = false,
      Client = client,
      Code = code,
      AuthTime = authTime,
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