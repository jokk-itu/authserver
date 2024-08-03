using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Endpoints.Responses;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;

internal class RegisterEndpoint
{
    public static async Task<IResult> HandleRegister(
        HttpContext httpContext,
        [FromServices] IRequestAccessor<RegisterRequest> requestAccessor,
        [FromServices] IRequestHandler<RegisterRequest, ProcessResult<Register.RegisterResponse, Unit>> requestHandler,
        CancellationToken cancellationToken)
    {
        var request = await requestAccessor.GetRequest(httpContext.Request);
        var response = await requestHandler.Handle(request, cancellationToken);
        return response.Match(
            result =>
            {
                return result.Match(
                    client =>
                    {
                        var body = new RegisterResponse
                        {
                            ClientId = client.ClientId,
                            ClientIdIssuedAt = DateTime.UtcNow,
                            ClientSecret = client.ClientSecret,
                            ClientSecretExpiresAt = client.ClientSecretExpiresAt,
                            RegistrationClientUri = client.RegistrationClientUri,
                            RegistrationAccessToken = client.RegistrationAccessToken,
                            ApplicationType = client.ApplicationType.GetDescription(),
                            TokenEndpointAuthMethod = client.TokenEndpointAuthMethod.GetDescription(),
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
                            SubjectType = client.SubjectType?.GetDescription(),
                            DefaultMaxAge = client.DefaultMaxAge,
                            DefaultAcrValues = client.DefaultAcrValues,
                            Contacts = client.Contacts,
                            AuthorizationCodeExpiration = client.AuthorizationCodeExpiration,
                            AccessTokenExpiration = client.AccessTokenExpiration,
                            RefreshTokenExpiration = client.RefreshTokenExpiration,
                            ClientSecretExpiration = client.ClientSecretExpiration,
                            JwksExpiration = client.JwksExpiration,
                            TokenEndpointAuthSigningAlg = client.TokenEndpointAuthSigningAlg.GetDescription(),
                            RequestObjectSigningAlg = client.RequestObjectSigningAlg?.GetDescription(),
                            RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg?.GetDescription(),
                            RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc?.GetDescription(),
                            UserinfoSignedResponseAlg = client.UserinfoSignedResponseAlg?.GetDescription(),
                            UserinfoEncryptedResponseAlg = client.UserinfoEncryptedResponseAlg?.GetDescription(),
                            UserinfoEncryptedResponseEnc = client.UserinfoEncryptedResponseEnc?.GetDescription(),
                            IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg?.GetDescription(),
                            IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg?.GetDescription(),
                            IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc?.GetDescription()
                        };

                        return request.Method == HttpMethod.Post
                            ? Results.Created(new Uri(client.RegistrationClientUri, UriKind.Absolute), body)
                            : Results.Ok(body);
                    },
                    _ => { return Results.NoContent(); });
            },
            error => Results.Extensions.OAuthBadRequest(error));
    }
}