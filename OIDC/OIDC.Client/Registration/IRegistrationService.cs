using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace OIDC.Client.Registration;

public interface IRegistrationService
{
    Task<RegistrationResponse> Register(OpenIdConnectOptions options, CancellationToken cancellationToken = default);
}