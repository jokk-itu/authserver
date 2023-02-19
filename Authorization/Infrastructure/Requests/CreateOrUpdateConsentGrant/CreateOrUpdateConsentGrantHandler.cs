using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Decoders.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateOrUpdateConsentGrant;
internal class CreateOrUpdateConsentGrantHandler : IRequestHandler<CreateOrUpdateConsentGrantCommand, CreateOrUpdateConsentGrantResponse>
{
  private readonly ICodeDecoder _codeDecoder;
  private readonly IValidator<CreateOrUpdateConsentGrantCommand> _validator;
  private readonly IdentityContext _identityContext;

  public CreateOrUpdateConsentGrantHandler(
    ICodeDecoder codeDecoder,
    IValidator<CreateOrUpdateConsentGrantCommand> validator,
    IdentityContext identityContext)
  {
    _codeDecoder = codeDecoder;
    _validator = validator;
    _identityContext = identityContext;
  }

  public async Task<CreateOrUpdateConsentGrantResponse> Handle(CreateOrUpdateConsentGrantCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken: cancellationToken);
    if (validationResult.IsError())
    {
      return new CreateOrUpdateConsentGrantResponse(validationResult.ErrorCode, validationResult.ErrorDescription, validationResult.StatusCode);
    }

    var consentGrant = await _identityContext
      .Set<ConsentGrant>()
      .Include(x => x.ConsentedClaims)
      .Include(x => x.ConsentedScopes)
      .Include(x => x.Client)
      .Include(x => x.User)
      .Where(x => x.Client.Id == request.ClientId)
      .Where(x => x.User.Id == request.UserId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var claims = await _identityContext
      .Set<Claim>()
      .Where(x => request.ConsentedClaims.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => request.ConsentedScopes.Any(y => y == x.Name))
      .ToListAsync(cancellationToken: cancellationToken);

    if (consentGrant is not null)
    {
      consentGrant.Updated = DateTime.UtcNow;
      consentGrant.ConsentedClaims = claims;
      consentGrant.ConsentedScopes = scopes;
    }
    else
    {
      var client = await _identityContext
        .Set<Client>()
        .SingleAsync(x => x.Id == request.ClientId, cancellationToken: cancellationToken);

      var user = await _identityContext
        .Set<User>()
        .SingleAsync(x => x.Id == request.UserId, cancellationToken: cancellationToken);

      consentGrant = new ConsentGrant
      {
        Client = client,
        User = user,
        ConsentedClaims = claims,
        ConsentedScopes = scopes,
        Updated = DateTime.UtcNow
      };

      await _identityContext
        .Set<ConsentGrant>()
        .AddAsync(consentGrant, cancellationToken);
    }
    await _identityContext.SaveChangesAsync(cancellationToken);

    return new CreateOrUpdateConsentGrantResponse(HttpStatusCode.OK);
  }
}
