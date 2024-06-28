using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Core.Discovery;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Enums;
using AuthServer.Helpers;
using AuthServer.TokenBuilders;
using AuthServer.TokenBuilders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthServer.Register.GetClient;

internal class GetRegisterRequestProcessor : IRequestProcessor<GetRegisterValidatedRequest, RegisterResponse>
{
	private readonly AuthorizationDbContext _authorizationDbContext;
	private readonly IUnitOfWork _unitOfWork;
	private readonly ITokenBuilder<RegistrationTokenArguments> _tokenBuilder;
	private readonly IOptionsSnapshot<DiscoveryDocument> _discoveryDocumentOptions;

	public GetRegisterRequestProcessor(
		AuthorizationDbContext authorizationDbContext,
		IUnitOfWork unitOfWork,
		ITokenBuilder<RegistrationTokenArguments> tokenBuilder,
		IOptionsSnapshot<DiscoveryDocument> discoveryDocumentOptions)
	{
		_authorizationDbContext = authorizationDbContext;
		_unitOfWork = unitOfWork;
		_tokenBuilder = tokenBuilder;
		_discoveryDocumentOptions = discoveryDocumentOptions;
	}

	public async Task<RegisterResponse> Process(GetRegisterValidatedRequest request, CancellationToken cancellationToken)
	{
		var registrationAccessToken = await _authorizationDbContext
			.Set<RegistrationToken>()
			.Where(t => t.Reference == request.RegistrationAccessToken)
			.SingleAsync(cancellationToken);

		registrationAccessToken.Revoke();

		var client = await _authorizationDbContext
			.Set<Client>()
			.Where(c => c.Id == request.ClientId)
			.Include(c => c.GrantTypes)
			.Include(c => c.ResponseTypes)
			.Include(c => c.RedirectUris)
			.Include(c => c.PostLogoutRedirectUris)
			.Include(c => c.RequestUris)
			.Include(c => c.Contacts)
			.Include(c => c.Scopes)
			.SingleAsync(cancellationToken);

		string? plainSecret = null;
		if (client.TokenEndpointAuthMethod != TokenEndpointAuthMethod.None)
		{
			plainSecret = CryptographyHelper.GetRandomString(32);
			var hashedSecret = BCrypt.HashPassword(plainSecret, BCrypt.GenerateSalt());
			client.SetSecret(hashedSecret);
		}

		// Persist the Client, as it is needed when building the token
		await _unitOfWork.SaveChanges();

		var registrationToken = await _tokenBuilder.BuildToken(
			new RegistrationTokenArguments
			{
				ClientId = client.Id
			}, cancellationToken);

		return new RegisterResponse
		{
			ClientId = client.Id,
			ClientIdIssuedAt = client.CreatedAt.Ticks,
			ClientSecret = plainSecret,
			ClientSecretExpiresAt = client.SecretExpiresAt?.Ticks ?? 0,
			RegistrationClientUri = $"{_discoveryDocumentOptions.Value.RegistrationEndpoint}?clientId={client.Id}",
			RegistrationAccessToken = registrationToken,
			ApplicationType = client.ApplicationType,
			TokenEndpointAuthMethod = client.TokenEndpointAuthMethod,
			ClientName = client.Name,
			GrantTypes = client.GrantTypes.Select(gt => gt.Name).ToList(),
			Scope = client.Scopes.Select(s => s.Name).ToList(),
			ResponseTypes = client.ResponseTypes.Select(rt => rt.Name).ToList(),
			RedirectUris = client.Scopes.Select(s => s.Name).ToList(),
			PostLogoutRedirectUris = client.Scopes.Select(s => s.Name).ToList(),
			RequestUris = client.Scopes.Select(s => s.Name).ToList(),
			BackchannelLogoutUri = client.BackchannelLogoutUri,
			ClientUri = client.ClientUri,
			PolicyUri = client.PolicyUri,
			TosUri = client.TosUri,
			InitiateLoginUri = client.InitiateLoginUri,
			LogoUri = client.LogoUri,
			Jwks = string.IsNullOrEmpty(client.JwksUri) ? client.Jwks : null,
			JwksUri = client.JwksUri,
			RequireSignedRequestObject = client.RequireSignedRequestObject,
			RequireReferenceToken = client.RequireReferenceToken,
			SubjectType = client.SubjectType,
			DefaultMaxAge = client.DefaultMaxAge,
			DefaultAcrValues = client.DefaultAcrValues?.Split(' ') ?? [],
			Contacts = client.Contacts.Select(c => c.Email).ToList(),
			AuthorizationCodeExpiration = client.AuthorizationCodeExpiration,
			AccessTokenExpiration = client.AccessTokenExpiration,
			RefreshTokenExpiration = client.RefreshTokenExpiration,
			ClientSecretExpiration = client.SecretExpiration,
			JwksExpiration = client.JwksExpiration,
			TokenEndpointAuthSigningAlg = client.TokenEndpointAuthSigningAlg,
			RequestObjectSigningAlg = client.RequestObjectSigningAlg,
			RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
			RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
			UserinfoSignedResponseAlg = client.UserinfoSignedResponseAlg,
			UserinfoEncryptedResponseAlg = client.UserinfoEncryptedResponseAlg,
			UserinfoEncryptedResponseEnc = client.UserinfoEncryptedResponseEnc,
			IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
			IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
			IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
			AuthorizationSignedResponseAlg = client.AuthorizationSignedResponseAlg,
			AuthorizationEncryptedResponseAlg = client.AuthorizationEncryptedResponseAlg,
			AuthorizationEncryptedResponseEnc = client.AuthorizationEncryptedResponseEnc
		};
	}
}