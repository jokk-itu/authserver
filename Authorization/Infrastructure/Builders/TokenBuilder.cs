using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application;
using Infrastructure.Services.Abstract;

namespace Infrastructure.Builders;
public class TokenBuilder : ITokenBuilder
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly JwkManager _jwkManager;
    private readonly ResourceManager _resourceManager;
    private readonly IClaimService _claimService;

    public TokenBuilder(
      IdentityConfiguration identityConfiguration,
      JwkManager jwkManager,
      ResourceManager resourceManager,
      IClaimService claimService)
    { 
      _identityConfiguration = identityConfiguration;
      _jwkManager = jwkManager;
      _resourceManager = resourceManager;
      _claimService = claimService;
    }

    public async Task<string> BuildClientAccessToken(string clientId, ICollection<string> scopes, CancellationToken cancellationToken = default)
    {
      var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.AccessTokenExpiration);
      var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
      var audiences = resources.Select(x => x.Name).ToArray();
      var claims = new Dictionary<string, object>
      {
        { ClaimNameConstants.Aud, audiences },
        { ClaimNameConstants.Scope, string.Join(' ', scopes) },
        { ClaimNameConstants.ClientId, clientId },
        { ClaimNameConstants.Jti, Guid.NewGuid() }
      };
      return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildAccessToken(
      string clientId,
      ICollection<string> scopes,
      string userId,
      string sessionId,
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.AccessTokenExpiration);
        var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
        var audiences = resources.Select(x => x.Name).ToArray();
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Sub, userId },
          { ClaimNameConstants.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.ClientId, clientId },
          { ClaimNameConstants.Sid, sessionId },
          { ClaimNameConstants.Jti, Guid.NewGuid() }
        };
        return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildRefreshToken(
      string authorizationGrantId,
      string clientId,
      ICollection<string> scopes,
      string userId,
      string sessionId,
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.RefreshTokenExpiration);
        var resources = await _resourceManager.ReadResourcesAsync(scopes, cancellationToken);
        var audiences = resources.Select(x => x.Name).ToArray();
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Sub, userId },
          { ClaimNameConstants.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.ClientId, clientId },
          { ClaimNameConstants.Sid, sessionId },
          { ClaimNameConstants.GrantId, authorizationGrantId },
          { ClaimNameConstants.Jti, Guid.NewGuid() }
        };
        return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildIdToken(
      string authorizationGrantId,
      string clientId, 
      ICollection<string> scopes, 
      string nonce, 
      string userId, 
      string sessionId,
      DateTime authTime,
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.IdTokenExpiration);
        var audiences = new[] { clientId };
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.Sid, sessionId },
          { ClaimNameConstants.Nonce, nonce },
          { ClaimNameConstants.AuthTime, authTime },
          { ClaimNameConstants.GrantId, authorizationGrantId },
          { ClaimNameConstants.Jti, Guid.NewGuid() }
        };
        var userInfo = await _claimService
          .GetClaimsFromConsentGrant(userId, clientId, cancellationToken: cancellationToken);

        foreach (var (key, value) in userInfo)
        {
          claims.Add(key, value);
        }

        return GetSignedToken(claims, expires);
    }

    public string BuildLogoutToken(string clientId, string sessionId, string userId, CancellationToken cancellationToken = default)
    {
      var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.IdTokenExpiration);
      var audiences = new[] { clientId };
      var claims = new Dictionary<string, object>
      {
        { ClaimNameConstants.Aud, audiences },
        { ClaimNameConstants.Sid, sessionId },
        { ClaimNameConstants.Sub, userId },
        { ClaimNameConstants.Jti, Guid.NewGuid() },
        { ClaimNameConstants.Events, new Dictionary<string, object>
        {
          { "http://schemas.openid.net/event/backchannel-logout", new() }
        }}
      };
      return GetSignedToken(claims, expires, tokenType: "logout+jwt");
    }

    public string BuildResourceInitialAccessToken()
    {
        var expires = DateTime.UtcNow.AddSeconds(300);
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ResourceRegistration) },
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildResourceRegistrationAccessToken(string resourceId)
    {
        var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
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
          { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
          { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ClientRegistration) },
        };
        return GetSignedToken(claims, expires);
    }

    public string BuildClientRegistrationAccessToken(string clientId)
    {
      var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
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
        { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
        { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ScopeRegistration) },
      };
      return GetSignedToken(claims, expires);
    }

    public string BuildScopeRegistrationAccessToken(string scopeId)
    {
      var expires = DateTime.UnixEpoch.AddSeconds(2145993506);
      var claims = new Dictionary<string, object>
      {
        { ClaimNameConstants.Aud, AudienceConstants.IdentityProvider },
        { ClaimNameConstants.Scope, string.Join(' ', ScopeConstants.ScopeConfiguration) },
        { ClaimNameConstants.ScopeId, scopeId }
      };
      return GetSignedToken(claims, expires);
    }

    private string GetSignedToken(
      IDictionary<string, object> claims,
      DateTime expires,
      string tokenType = "JWT")
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
            Issuer = _identityConfiguration.Issuer,
            SigningCredentials = signingCredentials,
            TokenType = tokenType,
            Claims = claims
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private string GetEncryptedToken(
      IDictionary<string, object> claims,
      DateTime expires)
    {
      var signingKey = new RsaSecurityKey(_jwkManager.RsaCryptoServiceProvider)
      {
        KeyId = _jwkManager.KeyId
      };
      var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);

      var encryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfiguration.EncryptingKeySecret));
      var encryptingCredentials = new EncryptingCredentials(encryptingKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        IssuedAt = DateTime.UtcNow,
        Expires = expires,
        NotBefore = DateTime.UtcNow,
        Issuer = _identityConfiguration.Issuer,
        SigningCredentials = signingCredentials,
        EncryptingCredentials = encryptingCredentials,
        Claims = claims
      };
      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
}
