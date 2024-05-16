namespace AuthServer.Core.RequestProcessing;
public class ProcessResult<TValue, TError>
{
    public readonly TValue? Value;
    public readonly TError? Error;

    public ProcessResult(TValue value) => Value = value;
    public ProcessResult(TError error) => Error = error;

    public bool IsSuccess => Value != null;

    public static implicit operator ProcessResult<TValue, TError>(TValue value) => new(value);
    public static implicit operator ProcessResult<TValue, TError>(TError error) => new(error);

    public TResponse Match<TResponse>(
        Func<TValue, TResponse> success,
        Func<TError, TResponse> failure)
        => IsSuccess ? success(Value!) : failure(Error!);
}