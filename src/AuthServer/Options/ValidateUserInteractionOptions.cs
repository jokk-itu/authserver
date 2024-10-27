using AuthServer.Core;
using Microsoft.Extensions.Options;

namespace AuthServer.Options;

public class ValidateUserInteractionOptions : IValidateOptions<UserInteraction>
{
    public ValidateOptionsResult Validate(string? name, UserInteraction options)
    {
        if (string.IsNullOrEmpty(options.LoginUri) || !Uri.IsWellFormedUriString(options.LoginUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.LoginUri)} is not specified");
        }

        if (string.IsNullOrEmpty(options.ConsentUri) || !Uri.IsWellFormedUriString(options.ConsentUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ConsentUri)} is not specified");
        }

        if (string.IsNullOrEmpty(options.AccountSelectionUri) || !Uri.IsWellFormedUriString(options.AccountSelectionUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.AccountSelectionUri)} is not specified");
        }

        if (string.IsNullOrEmpty(options.EndSessionUri) ||
            !Uri.IsWellFormedUriString(options.EndSessionUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.EndSessionUri)} is not specified");
        }

        return ValidateOptionsResult.Success;
    }
}