using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace OAuthService.TokenFactories;

public class IdTokenFactory : ITokenFactory
{
    private readonly AuthenticationConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly UserManager<IdentityUser> _userManager;

    public IdTokenFactory(AuthenticationConfiguration configuration,
        TokenValidationParameters tokenValidationParameters,
        UserManager<IdentityUser> userManager)
    {
        _configuration = configuration;
        _tokenValidationParameters = tokenValidationParameters;
        _userManager = userManager;
    }

    public async Task<string> GenerateTokenAsync(string clientId, string redirectUri, IEnumerable<string> scopes,
        string nonce, string userId)
    {
        var iat = DateTimeOffset.UtcNow;
        var exp = iat + TimeSpan.FromSeconds(_configuration.IdTokenExpiration);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Aud, new Uri(redirectUri).Host),
            new(JwtRegisteredClaimNames.Iss, _configuration.Issuer),
            new(JwtRegisteredClaimNames.Iat, iat.ToString()),
            new(JwtRegisteredClaimNames.Exp, exp.ToString()),
            new(JwtRegisteredClaimNames.Nbf, iat.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.AuthTime, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
            new("oid", userId),
            new(JwtRegisteredClaimNames.Nonce, nonce),
            new("scope", scopes.Aggregate((elem, acc) => $"{acc} {elem}")),
            new("client_id", clientId)
        };
        var user = await _userManager.FindByIdAsync(userId);
        var userClaims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userClaims.Select(claim => new Claim(claim.Type, claim.Value)));
        claims.Add(new Claim("roles", JsonSerializer.Serialize(userRoles)));
        
        var secret = _configuration.TokenSecret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        return await Task.FromResult(token);
    }

    public Task<JwtSecurityToken> DecodeTokenAsync(string token)
    {
        new JwtSecurityTokenHandler()
            .ValidateToken(token, _tokenValidationParameters, out var validatedToken);
        return Task.FromResult((JwtSecurityToken)validatedToken);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        new JwtSecurityTokenHandler()
            .ValidateToken(token, _tokenValidationParameters, out _);
        return await Task.FromResult(true);
    }
}