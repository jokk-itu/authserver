using Application;
using Domain;
using Domain.Constants;
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.Options;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly ILogger<ConfigureJwtBearerOptions> _logger;
    private readonly InternalConfigurationManager _internalConfigurationManager;
    private readonly IdentityConfiguration _identityConfiguration;

    public ConfigureJwtBearerOptions(
      ILogger<ConfigureJwtBearerOptions> logger,
      InternalConfigurationManager internalConfigurationManager,
      IdentityConfiguration identityConfiguration)
    {
      _logger = logger;
      _internalConfigurationManager = internalConfigurationManager;
      _identityConfiguration = identityConfiguration;
    }

    public void Configure(string name, JwtBearerOptions options)
    {
        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
      options.Audience = _identityConfiguration.Issuer;
      options.Authority = _identityConfiguration.Issuer;
      options.ConfigurationManager = _internalConfigurationManager;
      options.Challenge = OpenIdConnectDefaults.AuthenticationScheme;
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ClockSkew = TimeSpan.FromSeconds(0)
      };
      options.SaveToken = true;
      options.Events = new JwtBearerEvents
      {
        OnAuthenticationFailed = context =>
        {
          _logger.LogError(context.Exception, "Authentication failed");
          return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
          var scopes = context.Principal?.Claims.Where(x => x.Type == ClaimNameConstants.Scope);
          _logger.LogInformation("User is not Authorized, with scopes {@Scopes}", scopes);
          return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
          _logger.LogInformation("Token is validated, with id {TokenId}, from {ValidFrom}, to {ValidTo}",
          context.SecurityToken.Id, context.SecurityToken.ValidFrom, context.SecurityToken.ValidTo);
          return Task.CompletedTask;
        }, 
        OnChallenge = _ => 
        { 
          _logger.LogInformation("Challenge response returned"); 
          return Task.CompletedTask;
        },
        OnMessageReceived = async context => 
        { 
          _logger.LogInformation("Initiating bearer validation");
          if (!string.IsNullOrWhiteSpace(context.Token) && !TokenHelper.IsStructuredToken(context.Token))
          {
            var identityContext = context.HttpContext.RequestServices.GetRequiredService<IdentityContext>();
            var isValidReferenceToken = await identityContext
              .Set<Token>()
              .Where(x => x.Reference == context.Token)
              .Where(x => x.RevokedAt == null)
              .AnyAsync(context.HttpContext.RequestAborted);

            if (isValidReferenceToken)
            {
              context.Success();
            }
          }
        }
      };
      options.Validate();
    }
}