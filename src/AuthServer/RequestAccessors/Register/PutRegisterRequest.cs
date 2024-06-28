namespace AuthServer.RequestAccessors.Register;

internal class PutRegisterRequest : RegisterRequest
{
	public required string ClientId { get; init; }
	public required string RegistrationAccessToken { get; init; }
}