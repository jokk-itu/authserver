using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetLoginToken;
public class GetLoginTokenHandler : IRequestHandler<GetLoginTokenQuery, GetLoginTokenResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly ICodeBuilder _codeBuilder;
  private readonly IValidator<GetLoginTokenQuery> _validator;

  public GetLoginTokenHandler(
    IdentityContext identityContext,
    ICodeBuilder codeBuilder,
    IValidator<GetLoginTokenQuery> validator)
  {
    _identityContext = identityContext;
    _codeBuilder = codeBuilder;
    _validator = validator;
  }

  public async Task<GetLoginTokenResponse> Handle(GetLoginTokenQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new GetLoginTokenResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);
    }

    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.UserName == request.Username, cancellationToken: cancellationToken);

    var loginCode = await _codeBuilder.BuildLoginCodeAsync(user.Id);
    var userToken = new UserToken
    {
      Value = loginCode,
      ExpiresAt = DateTime.UtcNow.AddMinutes(1)
    };
    user.UserTokens.Add(userToken);
    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    return new GetLoginTokenResponse(HttpStatusCode.OK)
    {
      LoginCode = loginCode
    };
  }
}