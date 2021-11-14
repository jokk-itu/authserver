using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace OAuthService.TokenFactories;

public class AuthorizationCodeTokenFactory : ITokenFactory
{
    private readonly AuthenticationConfiguration _configuration;

    public AuthorizationCodeTokenFactory(AuthenticationConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> CreateToken(ICollection<string> scopes, string redirect_uri, string client_id)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "some_id"),
            new Claim(JwtRegisteredClaimNames.Aud, _configuration.Audience),
            new Claim(JwtRegisteredClaimNames.Iss, _configuration.Issuer),
            new Claim("Client_Id", client_id),
            new Claim("Scopes", JsonSerializer.Serialize(scopes)),
            new Claim("Redirect_Uri", redirect_uri)
        };
        
        var secret = _configuration.CodeSecret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var securityToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddSeconds(_configuration.AuthorizationCodeTokenExpiration),
            signingCredentials: signingCredentials);
        var code = new JwtSecurityTokenHandler().WriteToken(securityToken);
        return await Task.FromResult(code);
    }
}