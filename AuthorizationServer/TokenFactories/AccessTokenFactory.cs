using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthorizationServer.Repositories;

namespace AuthorizationServer.TokenFactories;

public class AccessTokenFactory : ITokenFactory
{
    private readonly AuthenticationConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ResourceManager _resourceManager;

    public AccessTokenFactory(
        AuthenticationConfiguration configuration,
        TokenValidationParameters tokenValidationParameters,
        ResourceManager resourceManager)
    {
        _configuration = configuration;
        _tokenValidationParameters = tokenValidationParameters;
        _resourceManager = resourceManager;
    }

    public async Task<string> GenerateTokenAsync(string clientId, string redirectUri, ICollection<string> scopes,
        string userId)
    {
        var iat = DateTimeOffset.UtcNow;
        var exp = iat + TimeSpan.FromSeconds(_configuration.AccessTokenExpiration);
        var resources = await _resourceManager.FindResourcesByScopes(scopes);
        var aud = resources.Aggregate(string.Empty, (acc, r) => $"{acc} {r}");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Aud, aud),
            new Claim(JwtRegisteredClaimNames.Iss, _configuration.Issuer),
            new Claim(JwtRegisteredClaimNames.Iat, iat.ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString()),
            new Claim(JwtRegisteredClaimNames.Nbf, iat.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", scopes.Aggregate((elem, acc) => $"{acc} {elem}")),
            new Claim("client_id", clientId)
        };

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