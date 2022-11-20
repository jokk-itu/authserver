using System.Net;
using Application.Validation;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.HasUserConsent;
public class HasUserConsentHandler : IRequestHandler<HasUserConsentQuery, HasUserConsentResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<HasUserConsentQuery> _validator;

  public HasUserConsentHandler(
    IdentityContext identityContext,
    IValidator<HasUserConsentQuery> validator)
  {
    _identityContext = identityContext;
    _validator = validator;
  }

  public async Task<HasUserConsentResponse> Handle(HasUserConsentQuery request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new HasUserConsentResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedScopes)
      .SingleOrDefaultAsync(x => 
        x.Client.Id == request.ClientId 
        && x.User.UserName == request.Username, cancellationToken: cancellationToken);

    if (consentGrant is null)
      return new HasUserConsentResponse(HttpStatusCode.Redirect)
      {
        HasValidConsent = false
      };

    var areScopesStale = !request.Scopes.All(x => consentGrant.ConsentedScopes.Any(y => y.Name == x));
    return new HasUserConsentResponse(HttpStatusCode.Redirect)
    {
      HasValidConsent = !areScopesStale
    };
  }
}