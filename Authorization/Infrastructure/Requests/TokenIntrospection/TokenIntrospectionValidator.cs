using Application;
using Application.Validation;
using Domain.Constants;
using Domain.Enums;
using Domain;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenIntrospection;
public class TokenIntrospectionValidator : IValidator<TokenIntrospectionQuery>
{
  private readonly IdentityContext _identityContext;

  public TokenIntrospectionValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> ValidateAsync(TokenIntrospectionQuery value, CancellationToken cancellationToken = default)
  {
    if (!TokenTypeConstants.TokenTypes.Contains(value.TokenTypeHint))
    {
      return new ValidationResult(ErrorCode.UnsupportedTokenType, "the given token_type_hint is not recognized", HttpStatusCode.BadRequest);
    }

    if (string.IsNullOrWhiteSpace(value.Token))
    {
      return new ValidationResult(ErrorCode.InvalidRequest, "token must not be null or empty",
        HttpStatusCode.BadRequest);
    }

    var token = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == value.Token)
      .Include(x => (x as GrantToken).AuthorizationGrant)
      .ThenInclude(x => x.Client)
      .Include(x => (x as ClientToken).Client)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var (isClientAuthenticated, clientValidatedResult) = await AuthorizeClient(value, token, cancellationToken);
    var (isResourceAuthenticated, resourceValidatedResult) = await AuthorizeResource(value, token, cancellationToken);
    if (clientValidatedResult.IsError() && resourceValidatedResult.IsError())
    {
      if (isClientAuthenticated)
      {
        return clientValidatedResult;
      }

      if (isResourceAuthenticated)
      {
        return resourceValidatedResult;
      }

      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated",
        HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<(bool, ValidationResult)> AuthorizeClient(TokenIntrospectionQuery query, Token? token, CancellationToken cancellationToken)
  {
    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == query.ClientId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (client is null ||
        client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None
        && client.Secret != query.ClientSecret)
    {
      return (false, new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest));
    }

    var clientId = token switch
    {
      GrantToken grantToken => grantToken.AuthorizationGrant.Client.Id,
      ClientToken clientToken => clientToken.Client.Id,
      _ => null
    };

    if (token is not null && clientId != query.ClientId)
    {
      return (true, new ValidationResult(ErrorCode.UnauthorizedClient, "client_id does not match token", HttpStatusCode.BadRequest));
    }

    return (true, new ValidationResult(HttpStatusCode.OK));
  }

  private async Task<(bool, ValidationResult)> AuthorizeResource(TokenIntrospectionQuery query, Token? token, CancellationToken cancellationToken)
  {
    var resource = await _identityContext
      .Set<Resource>()
      .Where(x => x.Id == query.ClientId)
      .Where(x => x.Secret == query.ClientSecret)
      .Include(x => x.Scopes)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (resource is null)
    {
      return (false, new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest));
    }

    if (token is not null && !resource.Scopes
          .Select(x => x.Name)
          .Intersect(token.Scope?.Split(' ') ?? Array.Empty<string>())
          .Any())
    {
      return (true, new ValidationResult(ErrorCode.UnauthorizedClient, "client is not authorized for scope", HttpStatusCode.BadRequest));
    }

    return (true, new ValidationResult(HttpStatusCode.OK));
  }
}