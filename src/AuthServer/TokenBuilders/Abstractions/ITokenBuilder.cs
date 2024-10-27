namespace AuthServer.TokenBuilders.Abstractions;
public interface ITokenBuilder<in TArguments>
    where TArguments : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> BuildToken(TArguments arguments, CancellationToken cancellationToken);
}