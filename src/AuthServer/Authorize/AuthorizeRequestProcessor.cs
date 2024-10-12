using AuthServer.Authorize.Abstractions;
using AuthServer.Codes;
using AuthServer.Codes.Abstractions;
using AuthServer.Constants;
using AuthServer.Core.Request;
using AuthServer.Entities;
using AuthServer.Helpers;
using AuthServer.Repositories.Abstractions;

namespace AuthServer.Authorize;

internal class AuthorizeRequestProcessor : IRequestProcessor<AuthorizeValidatedRequest, string>
{
    private readonly IAuthorizationCodeEncoder _authorizationCodeEncoder;
    private readonly IAuthorizeUserAccessor _userAccessor;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;
    private readonly IClientRepository _clientRepository;

    public AuthorizeRequestProcessor(
        IAuthorizationCodeEncoder authorizationCodeEncoder,
        IAuthorizeUserAccessor userAccessor,
        IAuthorizationGrantRepository authorizationGrantRepository,
        IClientRepository clientRepository)
    {
        _authorizationCodeEncoder = authorizationCodeEncoder;
        _userAccessor = userAccessor;
        _authorizationGrantRepository = authorizationGrantRepository;
        _clientRepository = clientRepository;
    }

    public async Task<string> Process(AuthorizeValidatedRequest request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null)
        {
            var isPushedRequest = request.RequestUri.StartsWith(RequestUriConstants.RequestUriPrefix);
            if (isPushedRequest)
            {
                var reference = request.RequestUri[(RequestUriConstants.RequestUriPrefix.Length)..];
                await _clientRepository.RedeemAuthorizeMessage(reference, cancellationToken);
            }
        }

        var user = _userAccessor.GetUser();
        var authorizationGrant = (await _authorizationGrantRepository.GetActiveAuthorizationGrant(
            user.SubjectIdentifier, request.ClientId, cancellationToken))!;

        var authorizationCode = new AuthorizationCode(authorizationGrant);
        var nonce = new Nonce(request.Nonce, request.Nonce.Sha256(), authorizationGrant);

        authorizationGrant.AuthorizationCodes.Add(authorizationCode);
        authorizationGrant.Nonces.Add(nonce);

        var encodedAuthorizationCode = _authorizationCodeEncoder.EncodeAuthorizationCode(
            new EncodedAuthorizationCode
            {
                AuthorizationGrantId = authorizationGrant.Id,
                AuthorizationCodeId = authorizationCode.Id,
                Scope = request.Scope,
                RedirectUri = request.RedirectUri,
                CodeChallenge = request.CodeChallenge
            });

        authorizationCode.SetValue(encodedAuthorizationCode);

        return encodedAuthorizationCode;
    }
}