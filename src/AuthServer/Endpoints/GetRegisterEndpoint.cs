using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;

internal class GetRegisterEndpoint
{
	public static async Task<IResult> HandleGetRegister(
		HttpContext httpContext,
		[FromServices] IRequestAccessor<GetRegisterRequest> requestAccessor,
		[FromServices] IRequestHandler<GetRegisterRequest, RegisterResponse> requestHandler,
		CancellationToken cancellationToken)
	{
		var request = await requestAccessor.GetRequest(httpContext.Request);
		var response = await requestHandler.Handle(request, cancellationToken);
		return response.Match(
			client =>
				Results.Ok(new RegisterResponse
				{
					ClientId = client.ClientId,
					ClientIdIssuedAt = DateTime.UtcNow,
					ClientSecret = client.ClientSecret,
					ClientSecretExpiresAt = client.ClientSecretExpiresAt,
					RegistrationClientUri = client.RegistrationClientUri,
					RegistrationAccessToken = client.RegistrationAccessToken,
					ApplicationType = client.ApplicationType,
					TokenEndpointAuthMethod = client.TokenEndpointAuthMethod,
					ClientName = client.ClientName,
					GrantTypes = client.GrantTypes,
					Scope = client.Scope,
					ResponseTypes = client.ResponseTypes,
					RedirectUris = client.RedirectUris,
					PostLogoutRedirectUris = client.PostLogoutRedirectUris,
					RequestUris = client.RequestUris,
					BackchannelLogoutUri = client.BackchannelLogoutUri,
					ClientUri = client.ClientUri,
					PolicyUri = client.PolicyUri,
					TosUri = client.TosUri,
					InitiateLoginUri = client.InitiateLoginUri,
					LogoUri = client.LogoUri,
					Jwks = client.Jwks,
					JwksUri = client.JwksUri,
					RequireSignedRequestObject = client.RequireSignedRequestObject,
					RequireReferenceToken = client.RequireReferenceToken,
					SubjectType = client.SubjectType,
					DefaultMaxAge = client.DefaultMaxAge,
					DefaultAcrValues = client.DefaultAcrValues,
					Contacts = client.Contacts,
					AuthorizationCodeExpiration = client.AuthorizationCodeExpiration,
					AccessTokenExpiration = client.AccessTokenExpiration,
					RefreshTokenExpiration = client.RefreshTokenExpiration,
					ClientSecretExpiration = client.ClientSecretExpiration,
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
				}),
			error => Results.Extensions.OAuthBadRequest(error));
	}
}