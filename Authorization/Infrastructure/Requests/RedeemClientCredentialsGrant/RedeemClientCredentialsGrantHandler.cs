using System.Net;
using Application;
using Application.Validation;
using Infrastructure.Builders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantHandler : IRequestHandler<RedeemClientCredentialsGrantCommand, RedeemClientCredentialsGrantResponse>
{
  private readonly ITokenBuilder _tokenBuilder;
  private readonly IValidator<RedeemClientCredentialsGrantCommand> _validator;
  private readonly IdentityConfiguration _identityConfiguration;

  public RedeemClientCredentialsGrantHandler(
    ITokenBuilder tokenBuilder,
    IValidator<RedeemClientCredentialsGrantCommand> validator,
    IdentityConfiguration identityConfiguration)
  {
    _tokenBuilder = tokenBuilder;
    _validator = validator;
    _identityConfiguration = identityConfiguration;
  }

  public async Task<RedeemClientCredentialsGrantResponse> Handle(RedeemClientCredentialsGrantCommand request,
    CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new RedeemClientCredentialsGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription,
        validationResult.StatusCode);
    }

    var accessToken = await _tokenBuilder.BuildClientAccessToken(request.ClientId, request.Scope.Split(' '),
      cancellationToken: cancellationToken);
    return new RedeemClientCredentialsGrantResponse(HttpStatusCode.OK)
    {
      AccessToken = accessToken,
      ExpiresIn = _identityConfiguration.AccessTokenExpiration
    };
  }
}