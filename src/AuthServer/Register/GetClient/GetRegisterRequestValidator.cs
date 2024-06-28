using AuthServer.Core.Request;
using AuthServer.Repositories.Abstractions;
using AuthServer.RequestAccessors.Register;

namespace AuthServer.Register.GetClient;

internal class GetRegisterRequestValidator : IRequestValidator<GetRegisterRequest, GetRegisterValidatedRequest>
{
	private readonly ITokenRepository _tokenRepository;

	public GetRegisterRequestValidator(ITokenRepository tokenRepository)
	{
		_tokenRepository = tokenRepository;
	}

	public async Task<ProcessResult<GetRegisterValidatedRequest, ProcessError>> Validate(GetRegisterRequest request, CancellationToken cancellationToken)
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

		return new GetRegisterValidatedRequest
		{
			ClientId = clientId,
			RegistrationAccessToken = registrationAccessToken
		};
	}
}