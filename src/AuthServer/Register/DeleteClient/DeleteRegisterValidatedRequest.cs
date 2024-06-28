namespace AuthServer.Register.DeleteClient;

internal class DeleteRegisterValidatedRequest
{
    public required string ClientId { get; init; }
    public required string RegistrationAccessToken { get; init; }
}