namespace AuthServer.Core.RequestProcessing;
public record ProcessError(string Error, string ErrorDescription, ResultCode ResultCode);