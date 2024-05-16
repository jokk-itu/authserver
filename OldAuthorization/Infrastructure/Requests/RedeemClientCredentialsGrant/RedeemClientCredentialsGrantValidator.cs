using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.RedeemClientCredentialsGrant;
public class RedeemClientCredentialsGrantValidator : IValidator<RedeemClientCredentialsGrantCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IClientService _clientService;

  public RedeemClientCredentialsGrantValidator(
    IdentityContext identityContext,
    IClientService clientService)
  {
    _identityContext = identityContext;
    _clientService = clientService;
  }

  public async Task<ValidationResult> ValidateAsync(RedeemClientCredentialsGrantCommand value, CancellationToken cancellationToken = default)
  {
    if (value.ClientAuthentications.Count != 1)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "multiple or none client authentication methods detected",
        HttpStatusCode.BadRequest);
    }

    var clientAuthentication = value.ClientAuthentications.Single();

    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Scopes)
      .Include(x => x.GrantTypes)
      .SingleOrDefaultAsync(x => x.Id == clientAuthentication.ClientId, cancellationToken: cancellationToken);

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    var isClientSecretValid = !string.IsNullOrWhiteSpace(clientAuthentication.ClientSecret)
                              && BCrypt.CheckPassword(clientAuthentication.ClientSecret, client.Secret);

    if (!isClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    if (value.GrantType != GrantTypeConstants.ClientCredentials)
    {
      return new ValidationResult(ErrorCode.UnsupportedGrantType, "grant_type must be client_credentials", HttpStatusCode.BadRequest);
    }

    if (client.GrantTypes.All(x => x.Name != value.GrantType))
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for client_credentials", HttpStatusCode.BadRequest);
    }

    var scope = string.IsNullOrWhiteSpace(value.Scope) ? null : value.Scope.Split(' ');
    if (scope is null)
    {
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.BadRequest);
    }

    if (!scope.All(x => client.Scopes.Any(y => y.Name == x)))
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is unauthorized for scope", HttpStatusCode.BadRequest);
    }

    var resourceValidation = await _clientService.ValidateResources(value.Resource, value.Scope, cancellationToken);
    if (resourceValidation.IsError())
    {
      return new ValidationResult(
        resourceValidation.ErrorCode,
        resourceValidation.ErrorDescription,
        HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}
