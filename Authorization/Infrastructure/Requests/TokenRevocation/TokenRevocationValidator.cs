using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Domain.Enums;
using Infrastructure.Decoders.Token;
using Infrastructure.Decoders.Token.Abstractions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.TokenRevocation;
public class TokenRevocationValidator : IValidator<TokenRevocationCommand>
{
  private readonly IdentityContext _identityContext;
  private readonly IStructuredTokenDecoder _tokenDecoder;

  public TokenRevocationValidator(IdentityContext identityContext, IStructuredTokenDecoder tokenDecoder)
  {
    _identityContext = identityContext;
    _tokenDecoder = tokenDecoder;
  }

  public async Task<ValidationResult> ValidateAsync(TokenRevocationCommand value, CancellationToken cancellationToken = default)
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

    if (client is null)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    var isClientSecretValid = client.Secret == null
                              || !string.IsNullOrWhiteSpace(value.ClientSecret)
                              && BCrypt.CheckPassword(value.ClientSecret, client.Secret);

    if (!isClientSecretValid)
    {
      return new ValidationResult(ErrorCode.InvalidClient, "client could not be authenticated", HttpStatusCode.BadRequest);
    }

    string? clientId;
    if (TokenHelper.IsStructuredToken(value.Token))
    {
      clientId = await GetClientIdFromStructuredToken(value, cancellationToken);
    }
    else
    {
      clientId = await GetClientIdFromReferenceToken(value.Token, cancellationToken);
    }

    
    if (clientId is not null && clientId != value.ClientId)
    {
      return new ValidationResult(ErrorCode.UnauthorizedClient, "client_id does not match token", HttpStatusCode.BadRequest);
    }

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<string?> GetClientIdFromStructuredToken(TokenRevocationCommand command, CancellationToken cancellationToken)
  {
    var securityToken = await _tokenDecoder.Decode(command.Token, new StructuredTokenDecoderArguments
    {
      ClientId = command.ClientId,
      ValidateAudience = false,
      ValidateLifetime = false
    });

    var id = Guid.Parse(securityToken.Id);
    var query = await _identityContext
      .Set<RefreshToken>()
      .Where(x => x.Id == id)
      .Select(x => new
      {
        ClientId = x.AuthorizationGrant.Client.Id
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    return query?.ClientId;
  }

  private async Task<string?> GetClientIdFromReferenceToken(string token, CancellationToken cancellationToken)
  {
    var clientToken = await _identityContext
      .Set<Token>()
      .Where(x => x.Reference == token)
      .Select(x => new
      {
        ClientIdFromGrant = (x as GrantToken).AuthorizationGrant.Client.Id,
        ClientId = (x as ClientAccessToken).Client.Id
      })
      .SingleOrDefaultAsync(cancellationToken: cancellationToken);

    return clientToken?.ClientIdFromGrant ?? clientToken?.ClientId;
  }
}