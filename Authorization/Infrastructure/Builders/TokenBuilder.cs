using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Infrastructure.Builders;
public class TokenBuilder : ITokenBuilder
{
    private readonly ILogger<TokenBuilder> _logger;
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly IOptions<JwtBearerOptions> _jwtBearerOptions;
    private readonly JwkManager _jwkManager;
    private readonly ResourceManager _resourceManager;

    public TokenBuilder(
      ILogger<TokenBuilder> logger,
      IdentityConfiguration identityConfiguration,
      IOptions<JwtBearerOptions> jwtBearerOptions,
      JwkManager jwkManager,
      ResourceManager resourceManager)
    {
        _logger = logger;
        _identityConfiguration = identityConfiguration;
        _jwtBearerOptions = jwtBearerOptions;
        _jwkManager = jwkManager;
        _resourceManager = resourceManager;
    }

    public async Task<string> BuildAccessTokenAsync(string clientId, ICollection<string> scopes, string userId,
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.AccessTokenExpiration);
        var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
        var audiences = resources.Select(x => x.Id).ToArray();
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Sub, userId },
          { JwtRegisteredClaimNames.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.ClientId, clientId }
        };
        return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildRefreshTokenAsync(string clientId, ICollection<string> scopes, string userId,
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.RefreshTokenExpiration);
        var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
        var audiences = resources.Select(x => x.Id).ToArray();
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Sub, userId },
          { JwtRegisteredClaimNames.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.ClientId, clientId }
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildIdToken(string clientId, ICollection<string> scopes, string nonce, string userId)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.IdTokenExpiration);
        var audiences = new[] { clientId };
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Sub, userId },
          { JwtRegisteredClaimNames.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.ClientId, clientId },
          { JwtRegisteredClaimNames.Nonce, nonce }
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildResourceInitialAccessToken()
    {
        var expires = DateTime.UtcNow.AddSeconds(300);
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ResourceRegistration) },
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildResourceRegistrationAccessToken(string resourceId)
    {
        var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ResourceConfiguration) },
          { ClaimNameConstants.ResourceId, resourceId }
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildClientInitialAccessToken()
    {
        var expires = DateTime.UtcNow.AddSeconds(300);
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ClientRegistration) },
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildClientRegistrationAccessToken(string clientId)
    {
      var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
        var claims = new Dictionary<string, object>
        {
          { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ClientConfiguration) },
          { ClaimNameConstants.ClientId, clientId }
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildScopeInitialAccessToken()
    {
      var expires = DateTime.UtcNow.AddSeconds(300);
      var claims = new Dictionary<string, object>
      {
        { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
        { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ScopeRegistration) },
      };
      return GetSignedToken(claims, expires);
    }

    public string BuildScopeRegistrationAccessToken(string scopeId)
    {
      var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
      var claims = new Dictionary<string, object>
      {
        { JwtRegisteredClaimNames.Aud, AudienceConstants.IdentityProvider },
        { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ScopeConfiguration) },
        { ClaimNameConstants.ScopeId, scopeId }
      };
      return GetSignedToken(claims, expires);
    }

    protected string GetSignedToken(
      IDictionary<string, object> claims,
      DateTime expires)
    {
        var key = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
        {
            KeyId = _jwkManager.KeyId
        };
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = DateTime.UtcNow,
            Expires = expires,
            NotBefore = DateTime.UtcNow,
            Issuer = _identityConfiguration.InternalIssuer,
            SigningCredentials = signingCredentials,
            Claims = claims
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    public JwtSecurityToken? DecodeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var signingKeys = _jwkManager.Jwks
          .Select(x => new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider) { KeyId = x.KeyId.ToString() })
          .ToList();

        var tokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKeys = signingKeys,
            ValidIssuer = _jwtBearerOptions.Value.Authority,
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true
        };

        try
        {
            new JwtSecurityTokenHandler()
              .ValidateToken(token, tokenValidationParameters, out var validatedToken);
            return validatedToken as JwtSecurityToken;
        }
        catch (SecurityTokenException exception)
        {
            _logger.LogError(exception, "Token {@token} is invalid", token);
            return null;
        }
    }
}
