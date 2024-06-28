using AuthServer.Entities;

namespace AuthServer.Repositories.Abstractions;

internal interface ITokenRepository
{
	Task<RegistrationToken?> GetRegistrationToken(string registrationAccessToken, CancellationToken cancellationToken);
}