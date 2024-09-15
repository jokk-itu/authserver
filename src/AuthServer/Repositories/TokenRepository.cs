using AuthServer.Core;
using AuthServer.Entities;
using AuthServer.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Repositories;

internal class TokenRepository : ITokenRepository
{
	private readonly AuthorizationDbContext _authorizationDbContext;

	public TokenRepository(AuthorizationDbContext authorizationDbContext)
	{
		_authorizationDbContext = authorizationDbContext;
	}

	/// <inheritdoc/>
	public async Task<RegistrationToken?> GetActiveRegistrationToken(string registrationAccessToken, CancellationToken cancellationToken)
	{
		return await _authorizationDbContext
			.Set<RegistrationToken>()
			.Where(t => t.Reference == registrationAccessToken)
			.Where(Token.IsActive)
			.OfType<RegistrationToken>()
			.Include(t => t.Client)
			.SingleOrDefaultAsync(cancellationToken);
	}
}