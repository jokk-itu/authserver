namespace AuthServer.Authorize.Abstractions;
public interface IAcrClaimMapper
{
    /// <summary>
    /// Passes an Amr claim value and returns the equivalent Acr claim value.
    /// </summary>
    /// <param name="amr"></param>
    /// <returns></returns>
    string MapAmrClaimToAcr(IReadOnlyCollection<string> amr);
}