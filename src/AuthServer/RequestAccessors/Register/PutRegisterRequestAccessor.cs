using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using AuthServer.Core;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AuthServer.RequestAccessors.Register;

internal class PutRegisterRequestAccessor : IRequestAccessor<PutRegisterRequest>
{
	public async Task<PutRegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var json = await JsonNode.ParseAsync(httpRequest.Body);
		if (json is null)
		{
			throw new NotSupportedException("Supports only Content-Type: application/json");
		}
		
		var clientId = httpRequest.Query.GetValueOrEmpty(Parameter.ClientId);
		var registrationAccessToken = await httpRequest.HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Parameter.AccessToken) ?? string.Empty;
		var clientName = json.GetValueOrEmpty(Parameter.ClientName);
		var applicationType = json[Parameter.ApplicationType]?.GetValue<string>() ?? string.Empty;
		var subjectType = json[Parameter.Subject]?.GetValue<string>() ?? string.Empty;
		var defaultMaxAge = json[Parameter.DefaultMaxAge]?.GetValue<string>() ?? string.Empty;
		var tokenEndpointAuthMethod = json[Parameter.TokenEndpointAuthMethod]?.GetValue<string>() ?? string.Empty;
		var tokenEndpointAuthSigningAlg = json[Parameter.TokenEndpointAuthSigningAlg]?.GetValue<string>() ?? string.Empty;
		var jwks = json[Parameter.Jwks]?.GetValue<string>() ?? string.Empty;
		var jwksUri = json[Parameter.JwksUri]?.GetValue<string>() ?? string.Empty;
		var clientUri = json[Parameter.ClientUri]?.GetValue<string>() ?? string.Empty;
		var policyUri = json[Parameter.PolicyUri]?.GetValue<string>() ?? string.Empty;
		var tosUri = json[Parameter.TosUri]?.GetValue<string>() ?? string.Empty;
		var initiateLoginUri = json[Parameter.InitiateLoginUri]?.GetValue<string>() ?? string.Empty;
		var logoUri = json[Parameter.LogoUri]?.GetValue<string>() ?? string.Empty;
		var backchannelLogoutUri = json[Parameter.BackchannelLogoutUri]?.GetValue<string>() ?? string.Empty;
		var requireSignedRequestObject = json[Parameter.RequireSignedRequestObject]?.GetValue<string>() ?? string.Empty;
		var requireReferenceToken = json[Parameter.RequireReferenceToken]?.GetValue<string>() ?? string.Empty;
		var requestObjectEncryptionEnc = json[Parameter.RequestObjectEncryptionEnc]?.GetValue<string>() ?? string.Empty;
		var requestObjectEncryptionAlg = json[Parameter.RequestObjectEncryptionAlg]?.GetValue<string>() ?? string.Empty;
		var requestObjectSigningAlg = json[Parameter.RequestObjectSigningAlg]?.GetValue<string>() ?? string.Empty;
		var userinfoEncryptedResponseEnc = json[Parameter.UserinfoEncryptedResponseEnc]?.GetValue<string>() ?? string.Empty;
		var userinfoEncryptedResponseAlg = json[Parameter.UserinfoEncryptedResponseAlg]?.GetValue<string>() ?? string.Empty;
		var userinfoSignedResponseAlg = json[Parameter.UserinfoSignedResponseAlg]?.GetValue<string>() ?? string.Empty;
		var idTokenEncryptedResponseEnc = json[Parameter.IdTokenEncryptedResponseEnc]?.GetValue<string>() ?? string.Empty;
		var idTokenEncryptedResponseAlg = json[Parameter.IdTokenEncryptedResponseAlg]?.GetValue<string>() ?? string.Empty;
		var idTokenSignedResponseAlg = json[Parameter.IdTokenSignedResponseAlg]?.GetValue<string>() ?? string.Empty;
		var authorizationEncryptedResponseEnc = json[Parameter.AuthorizationEncryptedResponseEnc]?.GetValue<string>() ?? string.Empty;
		var authorizationEncryptedResponseAlg = json[Parameter.AuthorizationEncryptedResponseAlg]?.GetValue<string>() ?? string.Empty;
		var authorizationSignedResponseAlg = json[Parameter.AuthorizationSignedResponseAlg]?.GetValue<string>() ?? string.Empty;

		var authorizationCodeExpiration = json[Parameter.AuthorizationCodeExpiration]?.GetValue<string>() ?? string.Empty;
		var accessTokenExpiration = json[Parameter.AccessTokenExpiration]?.GetValue<string>() ?? string.Empty;
		var refreshTokenExpiration = json[Parameter.RefreshTokenExpiration]?.GetValue<string>() ?? string.Empty;
		var clientSecretExpiration = json[Parameter.ClientSecretExpiration]?.GetValue<string>() ?? string.Empty;
		var jwksExpiration = json[Parameter.JwksExpiration]?.GetValue<string>() ?? string.Empty;

		var defaultAcrValues = json[Parameter.DefaultAcrValues]?.GetValue<string>()?.Split(' ') ?? [];
		var scope = json[Parameter.Scope]?.GetValue<string>()?.Split(' ') ?? [];

		var redirectUris = json[Parameter.RedirectUris]?.GetValue<string[]>() ?? [];
		var postLogoutRedirectUris = json[Parameter.PostLogoutRedirectUris]?.GetValue<string[]>() ?? [];
		var requestUris = json[Parameter.RequestUris]?.GetValue<string[]>() ?? [];
		var responseTypes = json[Parameter.ResponseTypes]?.GetValue<string[]>() ?? [];
		var grantTypes = json[Parameter.GrantTypes]?.GetValue<string[]>() ?? [];
		var contacts = json[Parameter.Contacts]?.GetValue<string[]>() ?? [];

		return new PutRegisterRequest
		{
			ClientId = clientId,
			RegistrationAccessToken = registrationAccessToken,
			ClientName = clientName,
			ApplicationType = applicationType,
			SubjectType = subjectType,
			DefaultMaxAge = defaultMaxAge,
			TokenEndpointAuthMethod = tokenEndpointAuthMethod,
			TokenEndpointAuthSigningAlg = tokenEndpointAuthSigningAlg,
			Jwks = jwks,
			JwksUri = jwksUri,
			ClientUri = clientUri,
			PolicyUri = policyUri,
			TosUri = tosUri,
			InitiateLoginUri = initiateLoginUri,
			LogoUri = logoUri,
			BackchannelLogoutUri = backchannelLogoutUri,
			RequireSignedRequestObject = requireSignedRequestObject,
			RequireReferenceToken = requireReferenceToken,
			RequestObjectEncryptionEnc = requestObjectEncryptionEnc,
			RequestObjectEncryptionAlg = requestObjectEncryptionAlg,
			RequestObjectSigningAlg = requestObjectSigningAlg,
			UserinfoEncryptedResponseEnc = userinfoEncryptedResponseEnc,
			UserinfoEncryptedResponseAlg = userinfoEncryptedResponseAlg,
			UserinfoSignedResponseAlg = userinfoSignedResponseAlg,
			IdTokenEncryptedResponseEnc = idTokenEncryptedResponseEnc,
			IdTokenEncryptedResponseAlg = idTokenEncryptedResponseAlg,
			IdTokenSignedResponseAlg = idTokenSignedResponseAlg,
			AuthorizationEncryptedResponseEnc = authorizationEncryptedResponseEnc,
			AuthorizationEncryptedResponseAlg = authorizationEncryptedResponseAlg,
			AuthorizationSignedResponseAlg = authorizationSignedResponseAlg,
			AuthorizationCodeExpiration = authorizationCodeExpiration,
			AccessTokenExpiration = accessTokenExpiration,
			RefreshTokenExpiration = refreshTokenExpiration,
			ClientSecretExpiration = clientSecretExpiration,
			JwksExpiration = jwksExpiration,
			DefaultAcrValues = defaultAcrValues,
			Scope = scope,
			RedirectUris = redirectUris,
			PostLogoutRedirectUris = postLogoutRedirectUris,
			RequestUris = requestUris,
			ResponseTypes = responseTypes,
			GrantTypes = grantTypes,
			Contacts = contacts
		};
	}
}