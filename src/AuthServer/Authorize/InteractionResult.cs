using AuthServer.Core.Request;

namespace AuthServer.Authorize;
internal class InteractionResult
{
    public static readonly InteractionResult LoginResult = new(){ Error = AuthorizeError.LoginRequired};
    public static readonly InteractionResult ConsentResult = new(){ Error = AuthorizeError.ConsentRequired};
    public static readonly InteractionResult SelectAccountResult = new(){ Error = AuthorizeError.AccountSelectionRequired};
    public static readonly InteractionResult UnmetAuthenticationRequirementResult = new(){ Error = AuthorizeError.UnmetAuthenticationRequirement};

    private InteractionResult()
    {
    }

    public ProcessError? Error { get; private init; }
    public string? SubjectIdentifier { get; private init; }

    public bool IsSuccessful => Error is null && !string.IsNullOrEmpty(SubjectIdentifier);

    public static InteractionResult Success(string subjectIdentifier)
    {
        return new InteractionResult
        {
            SubjectIdentifier = subjectIdentifier,
        };
    }
}
