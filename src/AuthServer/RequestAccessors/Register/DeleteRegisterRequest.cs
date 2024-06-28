namespace AuthServer.RequestAccessors.Register;

internal class DeleteRegisterRequest
{
	public required string ClientId { get; init; }
	public required string RegistrationAccessToken { get; init; }
}