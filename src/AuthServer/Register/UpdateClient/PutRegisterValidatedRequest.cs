namespace AuthServer.Register.UpdateClient;

internal class PutRegisterValidatedRequest : RegisterValidatedRequest
{
    public required string ClientId { get; init; }
    public required string RegistrationAccessToken { get; init; }
}