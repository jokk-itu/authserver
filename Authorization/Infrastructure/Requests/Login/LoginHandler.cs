using System.Net;
using Application.Validation;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.Login;
public class LoginHandler : IRequestHandler<LoginQuery, LoginResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<LoginQuery> _validator;

  public LoginHandler(
    IdentityContext identityContext,
    IValidator<LoginQuery> validator)
  {
    _identityContext = identityContext;
    _validator = validator;
  }

  public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new LoginResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);
    }

    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.UserName == request.Username, cancellationToken: cancellationToken);

    return new LoginResponse(HttpStatusCode.OK)
    {
      UserId = user.Id
    };
  }
}