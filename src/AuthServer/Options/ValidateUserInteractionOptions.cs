using AuthServer.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Options;

public class ValidateUserInteractionOptions : IValidateOptions<UserInteraction>
{
    public ValidateOptionsResult Validate(string? name, UserInteraction options)
    {
        if (options.LoginUri.IsNullOrEmpty() || !Uri.IsWellFormedUriString(options.LoginUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.LoginUri)} is not specified");
        }

        if (options.ConsentUri.IsNullOrEmpty() || !Uri.IsWellFormedUriString(options.ConsentUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.ConsentUri)} is not specified");
        }

        if (options.AccountSelectionUri.IsNullOrEmpty() || !Uri.IsWellFormedUriString(options.AccountSelectionUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.AccountSelectionUri)} is not specified");
        }

        if (options.EndSessionUri.IsNullOrEmpty() ||
            !Uri.IsWellFormedUriString(options.EndSessionUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.EndSessionUri)} is not specified");
        }

        return ValidateOptionsResult.Success;
    }
}