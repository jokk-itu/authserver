using AuthServer.Constants;
using AuthServer.RequestAccessors.Authorize;

namespace AuthServer.Authorize.Abstract;

internal interface IAuthorizeInteractionProcessor
{
    /// <summary>
    /// Deduces the prompt of the current authorize request.
    /// The possible values are defined in <see cref="PromptConstants"/>.
    ///
    /// </summary>
    /// <param name="authorizeRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>prompt value.</returns>
    Task<string> ProcessForInteraction(AuthorizeRequest authorizeRequest, CancellationToken cancellationToken);
}