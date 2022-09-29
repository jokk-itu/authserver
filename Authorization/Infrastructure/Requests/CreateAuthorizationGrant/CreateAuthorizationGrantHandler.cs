using System.Net;
using Application.Validation;
using Domain;
using Domain.Enums;
using Infrastructure.Factories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Requests.CreateAuthorizationGrant;
public class CreateAuthorizationGrantHandler : IRequestHandler<CreateAuthorizationGrantCommand, CreateAuthorizationGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateAuthorizationGrantCommand> _validator;
  private readonly CodeFactory _codeFactory;
  private readonly UserManager<User> _userManager;

  public CreateAuthorizationGrantHandler(
    IdentityContext identityContext, 
    IValidator<CreateAuthorizationGrantCommand> validator,
    CodeFactory codeFactory,
    UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _validator = validator;
    _codeFactory = codeFactory;
    _userManager = userManager;
  }

  public async Task<CreateAuthorizationGrantResponse> Handle(CreateAuthorizationGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateAuthorizationGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var user = await _userManager.FindByNameAsync(request.Username);

    var code = await _codeFactory.GenerateCodeAsync(
      request.RedirectUri,
      request.Scopes,
      request.ClientId,
      request.CodeChallenge,
      user.Id,
      request.Nonce);

    await _identityContext
      .Set<Code>()
      .AddAsync(new Code
      {
        Value = code,
        CodeType = CodeType.AuthorizationCode,
        IsRedeemed = false
      }, cancellationToken: cancellationToken);

    await _identityContext
      .Set<Nonce>()
      .AddAsync(new Nonce
      {
        Value = request.Nonce
      }, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new CreateAuthorizationGrantResponse(HttpStatusCode.Redirect)
    {
      Code = code,
      State = request.State
    };
  }
}