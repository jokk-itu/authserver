namespace Application;

public static class ErrorCode
{
  /// <summary>
  /// The request is missing a required parameter.
  /// </summary>
  public const string InvalidRequest = "invalid_request";

  /// <summary>
  /// Client authentication failed.
  /// </summary>
  public const string InvalidClient = "invalid_client";

  /// <summary>
  /// The provided authorization grant or refresh token is 
  /// invalid, expired, revoked, does not match the redirection
  /// URI used in the authorization request, or was issued to another client.
  /// </summary>
  public const string InvalidGrant = "invalid_grant";

  /// <summary>
  /// The authenticated client is not authorized.
  /// </summary>
  public const string UnauthorizedClient = "unauthorized_client";

  /// <summary>
  /// The resource owner or authorization server denied the request.
  /// </summary>
  public const string AccessDenied = "access_denied";

  /// <summary>
  /// The authorization server does not support the given response type.
  /// </summary>
  public const string UnsupportedResponseType = "unsupported_response_type";

  /// <summary>
  /// The authorization grant type is not supported by the authorization server.
  /// </summary>
  public const string UnsupportedGrantType = "unsupported_grant_type";

  /// <summary>
  /// The requested scope is invalid, unknown, malformed, or
  /// exceeds the scope granted by the resource owner.
  /// </summary>
  public const string InvalidScope = "invalid_scope";

  /// <summary>
  /// Corresponds to a 500 status.
  /// </summary>
  public const string ServerError = "server_error";

  /// <summary>
  /// Corresponds to a 503 status.
  /// </summary>
  public const string TemporarilyUnavailable = "temporarily_unavailable";

  /// <summary>
  /// Possibly several properties are invalid when registering or configuring clients.
  /// </summary>
  public const string InvalidClientMetadata = "invalid_client_metadata";

  /// <summary>
  /// Possibly several properties are invalid when registering or configuring resources.
  /// </summary>
  public const string InvalidResourceMetadata = "invalid_resource_metadata";
}