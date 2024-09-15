using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using AuthServer.Authentication;
using AuthServer.Core;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace AuthServer.RequestAccessors.Register;

internal class RegisterRequestAccessor : IRequestAccessor<RegisterRequest>
{
	public async Task<RegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var method = new HttpMethod(httpRequest.Method);
        var clientId = httpRequest.Query.GetValueOrEmpty(Parameter.ClientId);
        var registrationAccessToken = method == HttpMethod.Post ? string.Empty : await httpRequest.HttpContext.GetTokenAsync(OAuthTokenAuthenticationDefaults.AuthenticationScheme, Parameter.AccessToken) ?? string.Empty;
        if (method == HttpMethod.Get || method == HttpMethod.Delete)
        {
            return new RegisterRequest
            {
				Method = method,
				ClientId = clientId,
				RegistrationAccessToken = registrationAccessToken
            };
        }

        var json = await JsonNode.ParseAsync(httpRequest.Body);
		if (json is null)
		{
			throw new NotSupportedException("Supports only Content-Type: application/json");
		}

		var clientName = json.GetValueOrEmpty(Parameter.ClientName);
		var applicationType = json.GetValueOrEmpty(Parameter.ApplicationType);
		var subjectType = json.GetValueOrEmpty(Parameter.Subject);
		var defaultMaxAge = json.GetValueOrEmpty(Parameter.DefaultMaxAge);
		var tokenEndpointAuthMethod = json.GetValueOrEmpty(Parameter.TokenEndpointAuthMethod);
		var tokenEndpointAuthSigningAlg = json.GetValueOrEmpty(Parameter.TokenEndpointAuthSigningAlg);
		var jwks = json.GetValueOrEmpty(Parameter.Jwks);
		var jwksUri = json.GetValueOrEmpty(Parameter.JwksUri);
		var clientUri = json.GetValueOrEmpty(Parameter.ClientUri);
		var policyUri = json.GetValueOrEmpty(Parameter.PolicyUri);
		var tosUri = json.GetValueOrEmpty(Parameter.TosUri);
		var initiateLoginUri = json.GetValueOrEmpty(Parameter.InitiateLoginUri);
		var logoUri = json.GetValueOrEmpty(Parameter.LogoUri);
		var backchannelLogoutUri = json.GetValueOrEmpty(Parameter.BackchannelLogoutUri);
		var requireSignedRequestObject = json.GetValueOrEmpty(Parameter.RequireSignedRequestObject);
		var requireReferenceToken = json.GetValueOrEmpty(Parameter.RequireReferenceToken);
        var requirePushedAuthorizationRequests = json.GetValueOrEmpty(Parameter.RequirePushedAuthorizationRequests);
		var requestObjectEncryptionEnc = json.GetValueOrEmpty(Parameter.RequestObjectEncryptionEnc);
		var requestObjectEncryptionAlg = json.GetValueOrEmpty(Parameter.RequestObjectEncryptionAlg);
		var requestObjectSigningAlg = json.GetValueOrEmpty(Parameter.RequestObjectSigningAlg);
		var userinfoEncryptedResponseEnc = json.GetValueOrEmpty(Parameter.UserinfoEncryptedResponseEnc);
		var userinfoEncryptedResponseAlg = json.GetValueOrEmpty(Parameter.UserinfoEncryptedResponseAlg);
		var userinfoSignedResponseAlg = json.GetValueOrEmpty(Parameter.UserinfoEncryptedResponseAlg);
		var idTokenEncryptedResponseEnc = json.GetValueOrEmpty(Parameter.IdTokenEncryptedResponseEnc);
		var idTokenEncryptedResponseAlg = json.GetValueOrEmpty(Parameter.IdTokenEncryptedResponseAlg);
		var idTokenSignedResponseAlg = json.GetValueOrEmpty(Parameter.IdTokenSignedResponseAlg);

		var authorizationCodeExpiration = json.GetValueOrEmpty(Parameter.AuthorizationCodeExpiration);
		var accessTokenExpiration = json.GetValueOrEmpty(Parameter.AccessTokenExpiration);
		var refreshTokenExpiration = json.GetValueOrEmpty(Parameter.RefreshTokenExpiration);
		var clientSecretExpiration = json.GetValueOrEmpty(Parameter.ClientSecretExpiration);
		var jwksExpiration = json.GetValueOrEmpty(Parameter.JwksExpiration);
        var requestUriExpiration = json.GetValueOrEmpty(Parameter.RequestUriExpiration);

		var defaultAcrValues = json.GetSpaceDelimitedValueOrEmpty(Parameter.DefaultAcrValues);
		var scope = json.GetSpaceDelimitedValueOrEmpty(Parameter.Scope);

		var redirectUris = json.GetCollectionValue(Parameter.RedirectUris);
		var postLogoutRedirectUris = json.GetCollectionValue(Parameter.PostLogoutRedirectUris);
		var requestUris = json.GetCollectionValue(Parameter.RequestUris);
		var responseTypes = json.GetCollectionValue(Parameter.ResponseTypes);
		var grantTypes = json.GetCollectionValue(Parameter.GrantTypes);
		var contacts = json.GetCollectionValue(Parameter.Contacts);

		return new RegisterRequest
        {
			Method = method,
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
			RequirePushedAuthorizationRequests = requirePushedAuthorizationRequests,
			RequestObjectEncryptionEnc = requestObjectEncryptionEnc,
			RequestObjectEncryptionAlg = requestObjectEncryptionAlg,
			RequestObjectSigningAlg = requestObjectSigningAlg,
			UserinfoEncryptedResponseEnc = userinfoEncryptedResponseEnc,
			UserinfoEncryptedResponseAlg = userinfoEncryptedResponseAlg,
			UserinfoSignedResponseAlg = userinfoSignedResponseAlg,
			IdTokenEncryptedResponseEnc = idTokenEncryptedResponseEnc,
			IdTokenEncryptedResponseAlg = idTokenEncryptedResponseAlg,
			IdTokenSignedResponseAlg = idTokenSignedResponseAlg,
			AuthorizationCodeExpiration = authorizationCodeExpiration,
			AccessTokenExpiration = accessTokenExpiration,
			RefreshTokenExpiration = refreshTokenExpiration,
			ClientSecretExpiration = clientSecretExpiration,
			JwksExpiration = jwksExpiration,
			RequestUriExpiration = requestUriExpiration,
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