namespace Infrastructure.Builders.Token.Abstractions;
public interface ITokenBuilder<in TArguments>
  where TArguments : class
{
  Task<string> BuildToken(TArguments arguments);
}