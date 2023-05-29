using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDC.Client.Settings;

namespace App;

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
    options.ClientUri = identity["ClientUri"];
    options.Authority = identity["Authority"];

    options.Prompt = new[] { "login", "consent" };
    options.ClientName = $"webapp-{Guid.NewGuid()}";
    options.GrantTypes = new[] { OpenIdConnectGrantTypes.RefreshToken, OpenIdConnectGrantTypes.AuthorizationCode };
    options.TokenEndpointAuthMethod = "client_secret_post";
    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
    options.Scope = new[] { "profile", "openid", "email", "phone", "weather:read", "identityprovider:userinfo" };
  }

  public void Configure(string name, IdentityProviderSettings options)
  {
    Configure(options);
  }
}
