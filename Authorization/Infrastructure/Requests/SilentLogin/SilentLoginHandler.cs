using System.Net;
using Application;
using Application.Validation;
using Domain.Constants;
using Infrastructure.Decoders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.SilentLogin;
public class SilentLoginHandler : IRequestHandler<SilentLoginQuery, SilentLoginResponse>
{
  private readonly IValidator<SilentLoginQuery> _validator;
  private readonly ITokenDecoder _tokenDecoder;

  public SilentLoginHandler(
    IValidator<SilentLoginQuery> validator,
    ITokenDecoder tokenDecoder)
  {
    _validator = validator;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<SilentLoginResponse> Handle(SilentLoginQuery request, CancellationToken cancellationToken)
  {
    var validatorResponse = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validatorResponse.IsError())
    {
      return new SilentLoginResponse(validatorResponse.ErrorCode, validatorResponse.ErrorDescription, validatorResponse.StatusCode);
    }

    var token = _tokenDecoder.DecodeSignedToken(request.IdTokenHint);
    if (token is null)
    {
      return new SilentLoginResponse(ErrorCode.ServerError, "something went wrong", HttpStatusCode.OK);
    }

    var userId = token.Claims.Single(x => x.Type == ClaimNameConstants.Sid).Value;
    return new SilentLoginResponse(HttpStatusCode.OK)
    {
      UserId = userId
    };
  }
}