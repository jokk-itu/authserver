namespace AuthServer.TokenBuilders.Abstractions;
public interface ITokenBuilder<in TArguments>
    where TArguments : notnull
{
    Task<string> BuildToken(TArguments arguments, CancellationToken cancellationToken);
}