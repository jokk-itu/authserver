using Application;
using Application.Validation;
using Domain.Constants;
using Domain;
using System.Net;
using Infrastructure.Helpers;
using Infrastructure.Requests.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenIntrospection;

public class TokenIntrospectionValidator : IValidator<TokenIntrospectionQuery>
{
  private readonly IdentityContext _identityContext;

  public TokenIntrospectionValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(TokenIntrospectionQuery value,
    CancellationToken cancellationToken = default)
  {
    if (!TokenTypeConstants.TokenTypes.Contains(value.TokenTypeHint))
    {
      return new ValidationResult(ErrorCode.UnsupportedTokenType, "the given token_type_hint is not recognized",
        HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.Token))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "token must not be null or empty",
        HttpStatusCode.BadRequest);
    }

    if (value.ClientAuthentications.Count != 1)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "multiple or none client authentication methods detected",
        HttpStatusCode.BadRequest);
    }

    var clientAuthentication = value.ClientAuthentications.Single();

    var token = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == value.Token)
      .Include(x => (x as GrantToken).AuthorizationGrant)
      .ThenInclude(x => x.Client)
      .Include(x => (x as ClientToken).Client)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var clientValidatedResult = await AuthorizeClient(clientAuthentication, token, cancellationToken);
    if (clientValidatedResult.IsError())
    {
      return clientValidatedResult;
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<ValidationResult> AuthorizeClient(ClientAuthentication clientAuthentication, Token? token,
    CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Include(x => x.Scopes)
      .Where(x => x.Id == clientAuthentication.ClientId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    var isClientSecretValid = client.Secret is null
                              || !string.IsNullOrWhiteSpace(clientAuthentication.ClientSecret)
                              && BCrypt.CheckPassword(clientAuthentication.ClientSecret, client.Secret);

    if (!isClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    var clientId = token switch
    {
      GrantToken grantToken => grantToken.AuthorizationGrant.Client.Id,
      ClientToken clientToken => clientToken.Client.Id,
      _ => null
    };

    if (token is not null && clientId != clientAuthentication.ClientId)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client_id does not match token", HttpStatusCode.BadRequest);
    }

    var scope = token?.Scope?.Split(' ') ?? Array.Empty<string>();
    if (!client.Scopes.Select(s => s.Name).Intersect(scope).Any())
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client is not authorized for scope", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}