namespace AuthServer.RequestAccessors.Register;

internal class GetRegisterRequest
{
	public required string ClientId { get; init; }
	public required string RegistrationAccessToken { get; init; }
}