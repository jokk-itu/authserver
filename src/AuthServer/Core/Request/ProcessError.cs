namespace AuthServer.Core.Request;
public record ProcessError(string Error, string ErrorDescription, ResultCode ResultCode)
{
    public IDictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { Parameter.Error, Error },
            { Parameter.ErrorDescription, ErrorDescription }
        };
    }
}