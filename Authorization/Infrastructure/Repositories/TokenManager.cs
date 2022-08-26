﻿using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;
public class TokenManager
{
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;
  private readonly ILogger<TokenManager> _logger;

  public TokenManager(IdentityContext identityContext, UserManager<User> userManager, ILogger<TokenManager> logger)
  {
    _identityContext = identityContext;
    _userManager = userManager;
    _logger = logger;
  }

  public async Task<bool> IsTokenRevokedAsync(long keyId, CancellationToken cancellationToken = default)
  {
    var token = await _identityContext
      .Set<Token>()
      .FindAsync(new object?[] { keyId }, cancellationToken: cancellationToken);

    if(token is null)
    {
      _logger.LogDebug("Token {KeyId} does not exist", keyId);
      return false;
    }

    return token.RevokedAt is not null && token.RevokedBy is not null;
  }

  public async Task<bool> RevokeTokenAsync(long keyId, string userId) 
  {
    var user = await _userManager.FindByIdAsync(userId);
    if (user is null)
      return false;

    var token = await _identityContext
      .Set<Token>()
      .SingleOrDefaultAsync(token => token.KeyId == keyId);

    if (token is null)
      return false;

    token.RevokedBy = user;
    token.RevokedAt = DateTime.UtcNow;
    var result = await _identityContext.SaveChangesAsync();
    return result > 0;
  }

  public async Task<bool> CreateTokenAsync(int tokenTypeId, string tokenValue) 
  {
    var tokenType = await _identityContext
      .Set<TokenType>()
      .SingleOrDefaultAsync(tokenType => tokenType.Id == tokenTypeId);

    if (tokenType is null)
      return false;

    var token = new Token
    {
      TokenType = tokenType,
      Value = tokenValue
    };

    await _identityContext.Set<Token>().AddAsync(token);
    var result = await _identityContext.SaveChangesAsync();
    return result > 0;
  }
}