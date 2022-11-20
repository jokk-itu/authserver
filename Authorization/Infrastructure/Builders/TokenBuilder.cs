using Domain.Constants;
using Infrastructure.Builders.Abstractions;
using Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application;
using Domain;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Builders;
public class TokenBuilder : ITokenBuilder
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly JwkManager _jwkManager;
    private readonly ResourceManager _resourceManager;
    private readonly UserManager<User> _userManager;

    public TokenBuilder(
      IdentityConfiguration identityConfiguration,
      JwkManager jwkManager,
      ResourceManager resourceManager,
      UserManager<User> userManager)
    { 
      _identityConfiguration = identityConfiguration;
      _jwkManager = jwkManager;
      _resourceManager = resourceManager;
      _userManager = userManager;
    }

    public async Task<string> BuildAccessTokenAsync(string clientId, ICollection<string> scopes, string userId, string sessionId,
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
          { ClaimNameConstants.Sid, sessionId }
        };
        return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildRefreshTokenAsync(string clientId, ICollection<string> scopes, string userId, string sessionId,
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
          { ClaimNameConstants.Sid, sessionId }
        };
        return GetSignedToken(claims, expires);
    }

    public async Task<string> BuildIdTokenAsync(
      string clientId, 
      ICollection<string> scopes, 
      string nonce, 
      string userId, 
      string sessionId, 
      CancellationToken cancellationToken = default)
    {
        var expires = DateTime.UtcNow.AddSeconds(_identityConfiguration.IdTokenExpiration);
        var audiences = new[] { clientId };
        var claims = new Dictionary<string, object>
        {
          { ClaimNameConstants.Sub, userId},
          { ClaimNameConstants.Aud, audiences },
          { ClaimNameConstants.Scope, string.Join(' ', scopes) },
          { ClaimNameConstants.Sid, sessionId },
          { ClaimNameConstants.Nonce, nonce }
        };
        var user = await _userManager.FindByIdAsync(userId);
        var consentedClaims = user.ConsentGrants?
          .Single(x => x.Client.Id == clientId)
          .ConsentedClaims.Select(x => x.Name) ?? new List<string>();

        foreach (var claimName in consentedClaims)
        {
          switch(claimName)
          {
            case ClaimNameConstants.Name:
              claims.Add(claimName, $"{user.FirstName} {user.LastName}");
              break;
            case ClaimNameConstants.GivenName:
              claims.Add(claimName, user.FirstName);
              break;
            case ClaimNameConstants.FamilyName:
              claims.Add(claimName, user.LastName);
              break;
            case ClaimNameConstants.Birthdate:
              claims.Add(claimName, user.Birthdate);
              break;
            case ClaimNameConstants.Email:
              claims.Add(claimName, user.Email);
              break;
            case ClaimNameConstants.Address:
              claims.Add(claimName, user.Address);
              break;
            case ClaimNameConstants.Locale:
              claims.Add(claimName, user.Locale);
              break;
            case ClaimNameConstants.Phone:
              claims.Add(claimName, user.PhoneNumber);
              break;
          }
        }
        return GetSignedToken(claims, expires);
    }

    public string BuildLoginToken(string userId, CancellationToken cancellationToken = default)
    {
      var expires = DateTime.UtcNow.AddSeconds(300);
      var audiences = new[] { AudienceConstants.IdentityProvider };
      var scopes = new[] { ScopeConstants.Prompt };
      var claims = new Dictionary<string, object>
      {
        { ClaimNameConstants.Sub, userId},
        { ClaimNameConstants.Aud, audiences },
        { ClaimNameConstants.Scope, string.Join(' ', scopes) }
      };
      return GetEncryptedToken(claims, expires);
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
        Issuer = _identityConfiguration.InternalIssuer,
        SigningCredentials = signingCredentials,
        EncryptingCredentials = encryptingCredentials,
        Claims = claims
      };
      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }
}
