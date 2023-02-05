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

    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

    var user = await _identityContext
      .Set<User>()
      .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);

    return new GetConsentModelResponse(HttpStatusCode.OK)
    {
      Claims = ClaimsHelper.MapToClaims(request.Scope.Split(' ')),
      ClientName = client.Name,
      GivenName = user.FirstName,
      PolicyUri = client.PolicyUri,
      TosUri = client.TosUri
    };
  }
}