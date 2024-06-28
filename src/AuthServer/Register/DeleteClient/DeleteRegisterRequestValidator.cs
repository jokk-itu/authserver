using AuthServer.Core.Request;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register.DeleteClient;

internal class DeleteRegisterRequestValidator : IRequestValidator<DeleteRegisterRequest, DeleteRegisterValidatedRequest>
{
    private readonly ITokenRepository _tokenRepository;

    public DeleteRegisterRequestValidator(
        ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<ProcessResult<DeleteRegisterValidatedRequest, ProcessError>> Validate(DeleteRegisterRequest request, CancellationToken cancellationToken)
    {
        /* ClientId is REQUIRED */
        var clientId = request.ClientId;
        if (string.IsNullOrEmpty(clientId))
        {
            // TODO invalid ClientId given
        }

        var registrationAccessToken = request.RegistrationAccessToken;
        if (string.IsNullOrEmpty(registrationAccessToken))
        {
            // TODO invalid RegistrationAccessToken given
        }

        var token = await _tokenRepository.GetRegistrationToken(registrationAccessToken, cancellationToken);
        if (token is null)
        {
            // TODO invalid RegistrationAccessToken given
        }
        else if (token.Client.Id != clientId)
        {
            // TODO invalid Client mismatch
        }

        return new DeleteRegisterValidatedRequest
        {
            ClientId = clientId,
            RegistrationAccessToken = registrationAccessToken
        };
    }
}