using AuthServer.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit.Abstractions;

namespace AuthServer.Tests.IntegrationTest;
public class JwksDocumentIntegrationTest : BaseIntegrationTest
{
    public JwksDocumentIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }

    [Fact]
    public async Task GetJwks_SignTokenUsingKeyResolver_ExpectKeyForSigningValidation()
    {
        // Arrange
        var jsonWebTokenHandler = new JsonWebTokenHandler();
        var signingKey = JwksDocument.GetTokenSigningKey();
        var now = DateTime.UtcNow;
        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = DiscoveryDocument.Issuer,
            Audience = "test-app",
            Expires = now.AddSeconds(60),
            IssuedAt = now,
            NotBefore = now,
            TokenType = "jwt",
            SigningCredentials = new SigningCredentials(signingKey.Key, signingKey.Alg.GetDescription())
        };
        var token = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);

        var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(DiscoveryDocument.JwksUri);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var jwks = JsonWebKeySet.Create(responseContent);

        // Act
        var tokenValidationResult = await jsonWebTokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = jwks.Keys,
            TryAllIssuerSigningKeys = false
        });

        // Assert
        Assert.Null(tokenValidationResult.Exception);
        Assert.True(tokenValidationResult.IsValid);
    }
}
