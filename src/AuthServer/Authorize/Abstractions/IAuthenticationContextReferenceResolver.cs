namespace AuthServer.Authorize.Abstractions;

public interface IAuthenticationContextReferenceResolver
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<string> ResolveAuthenticationContextReference(IReadOnlyCollection<string> authenticationMethodReferences, CancellationToken cancellationToken);
}