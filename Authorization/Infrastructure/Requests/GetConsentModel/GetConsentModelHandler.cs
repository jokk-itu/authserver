using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.GetConsentModel;
public class GetConsentModelHandler : IRequestHandler<GetConsentModelQuery, GetConsentModelResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<GetConsentModelQuery> _validator;

  public GetConsentModelHandler(
    IdentityContext identityContext,
    IValidator<GetConsentModelQuery> validator)
  {
    _identityContext = identityContext;
    _validator = validator;
  }

  public async Task<GetConsentModelResponse> Handle(GetConsentModelQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new GetConsentModelResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

    var user = await _identityContext
      .Set<User>()
      .Where(x => x.Id == request.UserId)
      .SingleAsync(cancellationToken: cancellationToken);

    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == request.ClientId)
      .SingleAsync(cancellationToken: cancellationToken);

    var consentedClaims = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => x.Client.Id == request.ClientId)
      .SelectMany(x => x.ConsentedClaims)
      .Select(x => x.Name)
      .ToListAsync(cancellationToken: cancellationToken);

    var claims = ClaimsHelper
      .MapToClaims(request.Scope.Split(' '))
      .Select(x => new ClaimDto
      {
        Name = x,
        IsConsented = consentedClaims.Contains(x)
      });

    return new GetConsentModelResponse(HttpStatusCode.OK)
    {
      Claims = claims,
      ClientName = client.Name,
      GivenName = user.FirstName,
      PolicyUri = client.PolicyUri,
      TosUri = client.TosUri
    };
  }
}