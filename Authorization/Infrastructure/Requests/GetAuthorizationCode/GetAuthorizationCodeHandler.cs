using System.Net;
using Application.Validation;
using Domain;
using Domain.Enums;
using Infrastructure.Factories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Requests.GetAuthorizationCode;
public class GetAuthorizationCodeHandler : IRequestHandler<GetAuthorizationCodeQuery, GetAuthorizationCodeResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<GetAuthorizationCodeQuery> _validator;
  private readonly CodeFactory _codeFactory;
  private readonly UserManager<User> _userManager;

  public GetAuthorizationCodeHandler(
    IdentityContext identityContext, 
    IValidator<GetAuthorizationCodeQuery> validator,
    CodeFactory codeFactory,
    UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _validator = validator;
    _codeFactory = codeFactory;
    _userManager = userManager;
  }

  public async Task<GetAuthorizationCodeResponse> Handle(GetAuthorizationCodeQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.IsValidAsync(request);
    if (validationResult.IsError())
      return new GetAuthorizationCodeResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

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

    return new GetAuthorizationCodeResponse(HttpStatusCode.Redirect)
    {
      Code = code,
      State = request.State
    };
  }
}