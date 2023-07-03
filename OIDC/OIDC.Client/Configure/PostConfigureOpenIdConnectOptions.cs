using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using OIDC.Client.Registration;

namespace OIDC.Client.Configure;
public class PostConfigureOpenIdConnectOptions : IPostConfigureOptions<OpenIdConnectOptions>
{
    private readonly IRegistrationService _registrationService;

    public PostConfigureOpenIdConnectOptions(
      IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public void PostConfigure(string name, OpenIdConnectOptions options)
    {
        var registrationResponse = _registrationService
          .Register(options)
          .GetAwaiter()
          .GetResult();

        options.ClientId = registrationResponse.ClientId;
        options.ClientSecret = registrationResponse.ClientSecret;

        options.TokenValidationParameters.ValidAudience = registrationResponse.ClientId;
    }
}