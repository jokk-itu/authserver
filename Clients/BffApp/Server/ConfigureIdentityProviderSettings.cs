using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Client.Settings;

namespace Server;

public class ConfigureIdentityProviderSettings : IConfigureNamedOptions<IdentityProviderSettings>
{
  private readonly IConfiguration _configuration;

  public ConfigureIdentityProviderSettings(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void Configure(IdentityProviderSettings options)
  {
    var identity = _configuration.GetSection("Identity");
    options.ClientId = identity["ClientId"];
    options.ClientSecret = identity["ClientSecret"];
    options.ClientName = "wasm";
    options.Authority = identity["Authority"];
    options.GrantTypes = new[] { OpenIdConnectGrantTypes.AuthorizationCode, OpenIdConnectGrantTypes.RefreshToken };
    options.ClientAuthenticationMethod = "client_secret_post";
    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
    options.Scope = new[] { "openid", "profile", "email", "phone", "weather:read", "identityprovider:userinfo" };
    options.Resource = $"{options.Authority} {_configuration.GetSection("WeatherService")["Url"]}";
  }

  public void Configure(string name, IdentityProviderSettings options)
  {
    Configure(options);
  }
}