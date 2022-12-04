using System.Security.Claims;
using Application;
using Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

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
        options.IncludeErrorDetails = true; //DEVELOP READY
        options.RequireHttpsMetadata = false; //DEVELOP READY
        options.Audience = AudienceConstants.IdentityProvider;
        options.Authority = _identityConfiguration.Issuer;
        options.ConfigurationManager = _internalConfigurationManager;
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
              var roles = context.Principal?.Claims.Where(x => x.Type == ClaimTypes.Role);
              _logger.LogInformation("User is not Authorized, with scopes {@Scopes}, in roles {@Roles}", scopes, roles);
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
            OnMessageReceived = _ =>
            {
              _logger.LogInformation("Initiating bearer validation");
              return Task.CompletedTask;
            }
        };
        options.Validate();
    }
}