using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using AuthServer.Core;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using AuthServer.Authentication.OAuthToken;

namespace AuthServer.RequestAccessors.Register;

internal class RegisterRequestAccessor : IRequestAccessor<RegisterRequest>
{
	public async Task<RegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var method = new HttpMethod(httpRequest.Method);
        var clientId = httpRequest.Query.GetValue(Parameter.ClientId);
        var registrationAccessToken = method == HttpMethod.Post ? null : await httpRequest.HttpContext.GetTokenAsync(OAuthTokenAuthenticationDefaults.AuthenticationScheme, Parameter.AccessToken);
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

		var clientName = json.GetValue(Parameter.ClientName);
		var applicationType = json.GetValue(Parameter.ApplicationType);
		var subjectType = json.GetValue(Parameter.SubjectType);
		var defaultMaxAge = json.GetValue(Parameter.DefaultMaxAge);
		var tokenEndpointAuthMethod = json.GetValue(Parameter.TokenEndpointAuthMethod);
		var tokenEndpointAuthSigningAlg = json.GetValue(Parameter.TokenEndpointAuthSigningAlg);
		var jwks = json.GetValue(Parameter.Jwks);

		var jwksUri = json.GetValue(Parameter.JwksUri);
		var clientUri = json.GetValue(Parameter.ClientUri);
		var policyUri = json.GetValue(Parameter.PolicyUri);
		var tosUri = json.GetValue(Parameter.TosUri);
		var initiateLoginUri = json.GetValue(Parameter.InitiateLoginUri);
		var logoUri = json.GetValue(Parameter.LogoUri);
		var backchannelLogoutUri = json.GetValue(Parameter.BackchannelLogoutUri);

		var requireSignedRequestObject = json.GetValue(Parameter.RequireSignedRequestObject);
		var requireReferenceToken = json.GetValue(Parameter.RequireReferenceToken);
        var requirePushedAuthorizationRequests = json.GetValue(Parameter.RequirePushedAuthorizationRequests);

		var requestObjectEncryptionEnc = json.GetValue(Parameter.RequestObjectEncryptionEnc);
		var requestObjectEncryptionAlg = json.GetValue(Parameter.RequestObjectEncryptionAlg);
		var requestObjectSigningAlg = json.GetValue(Parameter.RequestObjectSigningAlg);

		var userinfoEncryptedResponseEnc = json.GetValue(Parameter.UserinfoEncryptedResponseEnc);
		var userinfoEncryptedResponseAlg = json.GetValue(Parameter.UserinfoEncryptedResponseAlg);
		var userinfoSignedResponseAlg = json.GetValue(Parameter.UserinfoEncryptedResponseAlg);

		var idTokenEncryptedResponseEnc = json.GetValue(Parameter.IdTokenEncryptedResponseEnc);
		var idTokenEncryptedResponseAlg = json.GetValue(Parameter.IdTokenEncryptedResponseAlg);
		var idTokenSignedResponseAlg = json.GetValue(Parameter.IdTokenSignedResponseAlg);

		var authorizationCodeExpiration = json.GetValue(Parameter.AuthorizationCodeExpiration);
		var accessTokenExpiration = json.GetValue(Parameter.AccessTokenExpiration);
		var refreshTokenExpiration = json.GetValue(Parameter.RefreshTokenExpiration);
		var clientSecretExpiration = json.GetValue(Parameter.ClientSecretExpiration);
		var jwksExpiration = json.GetValue(Parameter.JwksExpiration);
        var requestUriExpiration = json.GetValue(Parameter.RequestUriExpiration);

		var defaultAcrValues = json.GetSpaceDelimitedValue(Parameter.DefaultAcrValues);
		var scope = json.GetSpaceDelimitedValue(Parameter.Scope);

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