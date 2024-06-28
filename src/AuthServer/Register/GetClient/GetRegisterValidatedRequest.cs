namespace AuthServer.Register.GetClient;

internal class GetRegisterValidatedRequest
{
	public required string ClientId { get; init; }
	public required string RegistrationAccessToken { get; init; }
}