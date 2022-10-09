using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
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

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext, 
    IValidator<CreateAuthorizationGrantCommand> validator,
    UserManager<User> userManager,
    ICodeBuilder codeBuilder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _userManager = userManager;
    _codeBuilder = codeBuilder;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateAuthorizationGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var user = await _userManager.FindByNameAsync(request.Username);
    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var session = await _identityContext
      .Set<Session>()
      .Include(x => x.Clients)
      .SingleOrDefaultAsync(x => x.User.UserName == request.Username, cancellationToken: cancellationToken) ?? new Session
      {
        Created = DateTime.Now,
        Updated = DateTime.Now,
        User = user,
        MaxAge = request.MaxAge,
        Clients = new[] { client }
      };

    if(session.Clients.All(x => x.Id != request.ClientId))
      session.Clients.Add(client);

    var authorizationCodeGrant = new AuthorizationCodeGrant
    {
      IsRedeemed = false,
      Client = client,
      Nonce = request.Nonce,
      Sessions = new[] { session }
    };

    await _identityContext
      .Set<AuthorizationCodeGrant>()
      .AddAsync(authorizationCodeGrant, cancellationToken: cancellationToken);

    var code = await _codeBuilder.BuildAuthorizationCodeAsync(
      authorizationCodeGrant.Id,
      request.CodeChallenge,
      request.CodeChallengeMethod,
      user.Id,
      client.Id,
      request.Scopes);

    authorizationCodeGrant.Code = code;

    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateAuthorizationGrantResponse(HttpStatusCode.Redirect)
    {
      Code = code,
      State = request.State
    };
  }
}