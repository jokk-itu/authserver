﻿using Application;
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

    var client = await _identityContext
      .Set<Client>()
      .Where(x => x.Id == value.ClientId)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    if (client is null
        || client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None
        && client.Secret != value.ClientSecret)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    var token = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == value.Token)
      .Include(x => (x as GrantAccessToken).AuthorizationGrant)
      .ThenInclude(x => x.Client)
      .Include(x => (x as ClientAccessToken).Client)
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    var clientId = token switch
    {
      GrantAccessToken grantAccessToken => grantAccessToken.AuthorizationGrant.Client.Id,
      ClientAccessToken clientAccessToken => clientAccessToken.Client.Id,
      _ => null
    };

    if (token is not null && clientId != value.ClientId)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client_id does not match token", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }
}