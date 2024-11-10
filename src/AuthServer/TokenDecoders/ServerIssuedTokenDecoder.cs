using AuthServer.Helpers;
using AuthServer.Options;
using AuthServer.TokenDecoders.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.TokenDecoders;

internal class ServerIssuedTokenDecoder : ITokenDecoder<ServerIssuedTokenDecodeArguments>
{
    private readonly ILogger<ServerIssuedTokenDecoder> _logger;
    private readonly IOptionsSnapshot<JwksDocument> _jwkDocumentOptions;
    private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

    public ServerIssuedTokenDecoder(
        ILogger<ServerIssuedTokenDecoder> logger,
        IOptionsSnapshot<JwksDocument> jwkDocumentOptions,
        IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
    {
        _logger = logger;
        _jwkDocumentOptions = jwkDocumentOptions;
        _discoveryDocumentOptions = discoveryDocumentOptions;
    }

    public async Task<JsonWebToken> Read(string token)
    {
        var handler = new JsonWebTokenHandler();
        if (TokenHelper.IsJws(token))
        {
            return handler.ReadJsonWebToken(token);
        }

        if (TokenHelper.IsJwe(token))
        {
            var parameters = new TokenValidationParameters
            {
                TokenDecryptionKeys = _jwkDocumentOptions.Value.EncryptionKeys.Select(x => x.Key)
            };

            var tokenValidationResult = await handler.ValidateTokenAsync(token, parameters);
            return (tokenValidationResult.SecurityToken as JsonWebToken)!;
        }

        throw new ArgumentException("Not a valid JWT", nameof(token));
    }

    public async Task<JsonWebToken?> Validate(string token, ServerIssuedTokenDecodeArguments arguments,
        CancellationToken cancellationToken)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = new TimeSpan(0),
            ValidTypes = arguments.TokenTypes,
            ValidIssuer = _discoveryDocumentOptions.Value.Issuer,
            ValidAudiences = arguments.Audiences,
            IssuerSigningKeys = _jwkDocumentOptions.Value.SigningKeys.Select(x => x.Key),
            TokenDecryptionKeys = _jwkDocumentOptions.Value.EncryptionKeys.Select(x => x.Key),
            ValidateLifetime = arguments.ValidateLifetime,
            ValidateAudience = arguments.Audiences.Count != 0,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true
        };

        var handler = new JsonWebTokenHandler();
        var validationResult = await handler.ValidateTokenAsync(token, tokenValidationParameters);

        if (!validationResult.IsValid)
        {
            _logger.LogInformation(validationResult.Exception, "Token validation failed");
            return null;
        }

        return (validationResult.SecurityToken as JsonWebToken)!;
    }
}