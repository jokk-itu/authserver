using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Helpers;
using Infrastructure.Mappers;
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
      return validationResult.ToResponse<GetConsentModelResponse>();
    }

    var response = await _identityContext
      .Set<ConsentGrant>()
      .Where(x => x.User.Id == request.UserId)
      .Where(x => x.Client.Id == request.ClientId)
      .Select(x => new
      {
        GivenName = x.User.FirstName,
        ClientName = x.Client.Name,
        PolicyUri = x.Client.PolicyUri,
        TosUri = x.Client.TosUri,
        ConsentedClaims = x.ConsentedClaims.Select(y => y.Name)
      })
      .SingleAsync(cancellationToken: cancellationToken);

    var claims = ClaimsHelper
      .MapToClaims(request.Scope.Split(' '))
      .Select(x => new ClaimDto
      {
        Name = x,
        IsConsented = response.ConsentedClaims.Contains(x)
      });

    return new GetConsentModelResponse(HttpStatusCode.OK)
    {
      Claims = claims,
      ClientName = response.ClientName,
      GivenName = response.GivenName,
      PolicyUri = response.PolicyUri,
      TosUri = response.TosUri
    };
  }
}