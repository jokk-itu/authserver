using System.Net;
using Application.Validation;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateConsentGrant;
public class CreateConsentGrantHandler : IRequestHandler<CreateConsentGrantCommand, CreateConsentGrantResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateConsentGrantCommand> _validator;
  private readonly UserManager<User> _userManager;

  public CreateConsentGrantHandler(
    IdentityContext identityContext,
    IValidator<CreateConsentGrantCommand> validator,
    UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _validator = validator;
    _userManager = userManager;
  }

  public async Task<CreateConsentGrantResponse> Handle(CreateConsentGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateConsentGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);

    var client = await _identityContext
      .Set<Client>()
      .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);
    
    var consentedClaims = await _identityContext
      .Set<Claim>()
      .Where(x => request.ConsentedClaims.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.Scopes.Contains(x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var user = await _userManager.FindByNameAsync(request.Username);

    var consentGrant = new ConsentGrant
    {
      Client = client,
      ConsentedClaims = consentedClaims,
      Scopes = scopes,
      User = user,
      IsRevoked = false,
      IssuedAt = DateTime.Now,
      Updated = DateTime.Now
    };
    await _identityContext
      .Set<ConsentGrant>()
      .AddAsync(consentGrant, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);

    return new CreateConsentGrantResponse(HttpStatusCode.OK)
    {
      Id = consentGrant.Id
    };
  }
}
