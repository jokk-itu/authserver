using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Requests.GetLoginToken;
public class GetLoginTokenHandler : IRequestHandler<GetLoginTokenQuery, GetLoginTokenResponse>
{
  private readonly UserManager<User> _userManager;
  private readonly ICodeBuilder _codeBuilder;
  private readonly IValidator<GetLoginTokenQuery> _validator;

  public GetLoginTokenHandler(
    UserManager<User> userManager,
    ICodeBuilder codeBuilder,
    IValidator<GetLoginTokenQuery> validator)
  {
    _userManager = userManager;
    _codeBuilder = codeBuilder;
    _validator = validator;
  }

  public async Task<GetLoginTokenResponse> Handle(GetLoginTokenQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
      return new GetLoginTokenResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);

    var user = await _userManager.FindByNameAsync(request.Username);
    var loginCode = await _codeBuilder.BuildLoginCodeAsync(user.Id);
    return new GetLoginTokenResponse(HttpStatusCode.OK)
    {
      LoginCode = loginCode
    };
  }
}