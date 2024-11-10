using AuthServer.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Nodes;
using AuthServer.Core;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using AuthServer.Authentication.OAuthToken;
using AuthServer.Constants;

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
			throw new NotSupportedException($"Only supports Content-Type: {MimeTypeConstants.Json}");
		}

		var clientName = json.GetStringValue(Parameter.ClientName);
		var applicationType = json.GetStringValue(Parameter.ApplicationType);
		var subjectType = json.GetStringValue(Parameter.SubjectType);
		var defaultMaxAge = json.GetStringValue(Parameter.DefaultMaxAge);
		var tokenEndpointAuthMethod = json.GetStringValue(Parameter.TokenEndpointAuthMethod);
		var tokenEndpointAuthSigningAlg = json.GetStringValue(Parameter.TokenEndpointAuthSigningAlg);
		var jwks = json.GetStringValue(Parameter.Jwks);

		var jwksUri = json.GetStringValue(Parameter.JwksUri);
		var clientUri = json.GetStringValue(Parameter.ClientUri);
		var policyUri = json.GetStringValue(Parameter.PolicyUri);
		var tosUri = json.GetStringValue(Parameter.TosUri);
		var initiateLoginUri = json.GetStringValue(Parameter.InitiateLoginUri);
		var logoUri = json.GetStringValue(Parameter.LogoUri);
		var backchannelLogoutUri = json.GetStringValue(Parameter.BackchannelLogoutUri);
        var sectorIdentifierUri = json.GetStringValue(Parameter.SectorIdentifierUri);

		var requireSignedRequestObject = json.GetBoolValue(Parameter.RequireSignedRequestObject);
		var requireReferenceToken = json.GetBoolValue(Parameter.RequireReferenceToken);
        var requirePushedAuthorizationRequests = json.GetBoolValue(Parameter.RequirePushedAuthorizationRequests);

		var requestObjectEncryptionEnc = json.GetStringValue(Parameter.RequestObjectEncryptionEnc);
		var requestObjectEncryptionAlg = json.GetStringValue(Parameter.RequestObjectEncryptionAlg);
		var requestObjectSigningAlg = json.GetStringValue(Parameter.RequestObjectSigningAlg);

		var userinfoEncryptedResponseEnc = json.GetStringValue(Parameter.UserinfoEncryptedResponseEnc);
		var userinfoEncryptedResponseAlg = json.GetStringValue(Parameter.UserinfoEncryptedResponseAlg);
		var userinfoSignedResponseAlg = json.GetStringValue(Parameter.UserinfoSignedResponseAlg);

		var idTokenEncryptedResponseEnc = json.GetStringValue(Parameter.IdTokenEncryptedResponseEnc);
		var idTokenEncryptedResponseAlg = json.GetStringValue(Parameter.IdTokenEncryptedResponseAlg);
		var idTokenSignedResponseAlg = json.GetStringValue(Parameter.IdTokenSignedResponseAlg);

		var authorizationCodeExpiration = json.GetIntValue(Parameter.AuthorizationCodeExpiration);
		var accessTokenExpiration = json.GetIntValue(Parameter.AccessTokenExpiration);
		var refreshTokenExpiration = json.GetIntValue(Parameter.RefreshTokenExpiration);
		var clientSecretExpiration = json.GetIntValue(Parameter.ClientSecretExpiration);
		var jwksExpiration = json.GetIntValue(Parameter.JwksExpiration);
        var requestUriExpiration = json.GetIntValue(Parameter.RequestUriExpiration);

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
			SectorIdentifierUri = sectorIdentifierUri,
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