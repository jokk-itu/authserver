using AuthServer.Authorize.Abstract;
using AuthServer.Codes;
using AuthServer.Codes.Abstractions;
using AuthServer.Entities;
using AuthServer.Repositories.Abstract;

namespace AuthServer.Authorize;

internal class AuthorizeProcessor : IAuthorizeProcessor
{
    private readonly IAuthorizationCodeEncoder _authorizationCodeEncoder;
    private readonly IUserAccessor _userAccessor;
    private readonly IAuthorizationGrantRepository _authorizationGrantRepository;

    public AuthorizeProcessor(
        IAuthorizationCodeEncoder authorizationCodeEncoder,
        IUserAccessor userAccessor,
        IAuthorizationGrantRepository authorizationGrantRepository)
    {
        _authorizationCodeEncoder = authorizationCodeEncoder;
        _userAccessor = userAccessor;
        _authorizationGrantRepository = authorizationGrantRepository;
    }

    public async Task<string> Process(AuthorizeValidatedRequest request, CancellationToken cancellationToken)
    {
        var user = _userAccessor.GetUser();
        long? maxAge = null;
        var isParsed = long.TryParse(request.MaxAge, out var parsedMaxAge);
        if (isParsed)
        {
            maxAge = parsedMaxAge;
        }

        var authorizationGrant = await _authorizationGrantRepository.CreateAuthorizationGrant(
            user.SubjectIdentifier, request.ClientId, maxAge, cancellationToken);

        var authorizationCode = new AuthorizationCode(authorizationGrant);
        var nonce = new Nonce(request.Nonce, authorizationGrant);

        authorizationGrant.AuthorizationCodes.Add(authorizationCode);
        authorizationGrant.Nonces.Add(nonce);

        var encodedAuthorizationCode = _authorizationCodeEncoder.EncodeAuthorizationCode(
            new EncodedAuthorizationCode
            {
                AuthorizationGrantId = authorizationGrant.Id,
                AuthorizationCodeId = authorizationCode.Id,
                NonceId = nonce.Id,
                Scope = request.Scope,
                RedirectUri = request.RedirectUri,
                CodeChallengeMethod = request.CodeChallengeMethod,
                CodeChallenge = request.CodeChallenge
            });

        authorizationCode.SetValue(encodedAuthorizationCode);

        return encodedAuthorizationCode;
    }
}