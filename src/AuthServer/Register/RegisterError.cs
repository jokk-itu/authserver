using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.Register;
internal class RegisterError
{
    public static readonly ProcessError InvalidClientId =
        new(ErrorCode.InvalidClientMetadata, "invalid client_id", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRegistrationAccessToken =
        new(ErrorCode.AccessDenied, "access_token is invalid", ResultCode.Unauthorized);

    public static readonly ProcessError MismatchingClientId =
        new(ErrorCode.AccessDenied, "client is not authorized with access_token", ResultCode.Forbidden);

	public static readonly ProcessError InvalidApplicationType = 
        new(ErrorCode.InvalidClientMetadata, "invalid application_type", ResultCode.BadRequest);

    public static readonly ProcessError InvalidTokenEndpointAuthMethod =
        new(ErrorCode.InvalidClientMetadata, "invalid token_endpoint_auth_method", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClientName =
        new(ErrorCode.InvalidClientMetadata, "invalid client_name", ResultCode.BadRequest);

    public static readonly ProcessError InvalidGrantTypes =
        new(ErrorCode.InvalidClientMetadata, "invalid grant_types", ResultCode.BadRequest);

    public static readonly ProcessError InvalidScope =
        new(ErrorCode.InvalidClientMetadata, "invalid scope", ResultCode.BadRequest);

    public static readonly ProcessError InvalidResponseTypes =
        new(ErrorCode.InvalidClientMetadata, "invalid response_types", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRedirectUris =
        new(ErrorCode.InvalidClientMetadata, "invalid redirect_uris", ResultCode.BadRequest);

    public static readonly ProcessError InvalidPostLogoutRedirectUris =
        new(ErrorCode.InvalidClientMetadata, "invalid post_logout_redirect_uris", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequestUris =
        new(ErrorCode.InvalidClientMetadata, "invalid request_uris", ResultCode.BadRequest);

    public static readonly ProcessError InvalidBackchannelLogoutUri =
        new(ErrorCode.InvalidClientMetadata, "invalid backchannel_logout_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClientUri =
        new(ErrorCode.InvalidClientMetadata, "invalid client_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidPolicyUri =
        new(ErrorCode.InvalidClientMetadata, "invalid policy_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidTosUri =
        new(ErrorCode.InvalidClientMetadata, "invalid tos_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidInitiateLoginUri =
        new(ErrorCode.InvalidClientMetadata, "invalid initiate_login_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidLogoUri =
        new(ErrorCode.InvalidClientMetadata, "invalid logo_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidJwksAndJwksUri =
        new(ErrorCode.InvalidClientMetadata, "jwks and jwks_uri are not both allowed", ResultCode.BadRequest);

    public static readonly ProcessError InvalidJwksOrJwksUri =
        new(ErrorCode.InvalidClientMetadata, "jwks or jwks_uri is required when token_endpoint_auth_method is private_key_jwt", ResultCode.BadRequest);

    public static readonly ProcessError InvalidJwks =
        new(ErrorCode.InvalidClientMetadata, "invalid jwks", ResultCode.BadRequest);

    public static readonly ProcessError InvalidJwksUri =
        new(ErrorCode.InvalidClientMetadata, "invalid jwks_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequireSignedRequestObject =
        new(ErrorCode.InvalidClientMetadata, "invalid require_signed_request_object", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequireReferenceToken =
        new(ErrorCode.InvalidClientMetadata, "invalid require_reference_token", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequirePushedAuthorizationRequests =
        new(ErrorCode.InvalidClientMetadata, "invalid require_pushed_authorization_requests", ResultCode.BadRequest);

    public static readonly ProcessError InvalidSubjectType =
        new(ErrorCode.InvalidClientMetadata, "invalid subject_type", ResultCode.BadRequest);

    public static readonly ProcessError InvalidDefaultMaxAge =
        new(ErrorCode.InvalidClientMetadata, "invalid default_max_age", ResultCode.BadRequest);

    public static readonly ProcessError InvalidDefaultAcrValues =
        new(ErrorCode.InvalidClientMetadata, "invalid default_acr_values", ResultCode.BadRequest);

    public static readonly ProcessError InvalidContacts =
        new(ErrorCode.InvalidClientMetadata, "invalid contacts", ResultCode.BadRequest);

    public static readonly ProcessError InvalidAuthorizationCodeExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid authorization_code_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidAccessTokenExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid access_token_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRefreshTokenExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid refresh_token_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidClientSecretExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid client_secret_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidJwksExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid jwks_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequestUriExpiration =
        new(ErrorCode.InvalidClientMetadata, "invalid request_uri_expiration", ResultCode.BadRequest);

    public static readonly ProcessError InvalidTokenEndpointAuthSigningAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid token_endpoint_auth_signing_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequestObjectSigningAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid request_object_signing_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequestObjectEncryptionAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid request_object_encryption_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidRequestObjectEncryptionEnc =
        new(ErrorCode.InvalidClientMetadata, "invalid request_object_encryption_enc", ResultCode.BadRequest);

    public static readonly ProcessError InvalidUserinfoSignedResponseAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid userinfo_signed_response_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidUserinfoEncryptedResponseAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid userinfo_encrypted_response_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidUserinfoEncryptedResponseEnc =
        new(ErrorCode.InvalidClientMetadata, "invalid userinfo_encrypted_response_enc", ResultCode.BadRequest);

    public static readonly ProcessError InvalidIdTokenSignedResponseAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid id_token_signed_response_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidIdTokenEncryptedResponseAlg =
        new(ErrorCode.InvalidClientMetadata, "invalid id_token_encrypted_response_alg", ResultCode.BadRequest);

    public static readonly ProcessError InvalidIdTokenEncryptedResponseEnc =
        new(ErrorCode.InvalidClientMetadata, "invalid id_token_encrypted_response_enc", ResultCode.BadRequest);

    public static readonly ProcessError InvalidSectorIdentifierUri =
        new(ErrorCode.InvalidClientMetadata, "invalid sector_identifier_uri", ResultCode.BadRequest);

    public static readonly ProcessError InvalidSectorDocument =
        new(ErrorCode.InvalidClientMetadata, "sector_identifier_uri does not point to all redirect_uris", ResultCode.BadRequest);
}