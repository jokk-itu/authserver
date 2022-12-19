using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantValidator : IValidator<RedeemClientCredentialsGrantCommand>
{
  private readonly IdentityContext _identityContext;

  public RedeemClientCredentialsGrantValidator(
    IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemClientCredentialsGrantCommand value, CancellationToken cancellationToken = default)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Scopes)
      .Include(x => x.GrantTypes)
      .SingleOrDefaultAsync(x => x.Id == value.ClientId && x.Secret == value.ClientSecret, cancellationToken: cancellationToken);

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client is invalid", HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.ClientCredentials)
    {
      return new ValidationResult(ErrorCode.UnsupportedGrantType, "grant_type must be client_credentials", HttpStatusCode.BadRequest);
    }

    if (client.GrantTypes.All(x => x.Name != value.GrantType))
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for client_credentials", HttpStatusCode.BadRequest);
    }

    if (!value.Scope.Split(' ').All(x => client.Scopes.Any(y => y.Name == x)))
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for scope", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}
