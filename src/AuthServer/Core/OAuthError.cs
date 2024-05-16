using System.Text.Json.Serialization;
using AuthServer.Core.RequestProcessing;

namespace AuthServer.Core;

public record OAuthError(string Error, string ErrorDescription)
{
    public static implicit operator OAuthError(ProcessError error) =>
        new(error.Error, error.ErrorDescription);

    [JsonPropertyName(Parameter.Error)] public string Error { get; private init; } = Error;

    [JsonPropertyName(Parameter.ErrorDescription)] public string ErrorDescription { get; private init; } = ErrorDescription;
}