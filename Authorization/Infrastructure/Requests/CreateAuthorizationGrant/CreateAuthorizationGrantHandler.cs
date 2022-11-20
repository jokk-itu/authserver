using System.Net;
using System.Security;
using Application.Validation;
using Domain;
using Domain.Constants;
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
  private readonly ITokenDecoder _tokenDecoder;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext, 
    IValidator<CreateAuthorizationGrantCommand> validator,
    UserManager<User> userManager,
    ICodeBuilder codeBuilder,
    ITokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _userManager = userManager;
    _codeBuilder = codeBuilder;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateAuthorizationGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var decryptedToken = _tokenDecoder.DecodeEncryptedToken(request.LoginToken);
    if (decryptedToken is null)
      throw new SecurityException("token should be decryptable");

    var userId = decryptedToken.Claims
      .Where(x => x.Type == ClaimNameConstants.Sub)
      .Select(x => x.Value)
      .Single();

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

    if(session.Clients.All(x => x.Id != request.ClientId))
      session.Clients.Add(client);

    var grantId = Guid.NewGuid().ToString();

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
      Nonce = request.Nonce,
      Session = session
    };

    await _identityContext
      .Set<AuthorizationCodeGrant>()
      .AddAsync(authorizationCodeGrant, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateAuthorizationGrantResponse(HttpStatusCode.Redirect)
    {
      Code = code,
      State = request.State
    };
  }
}