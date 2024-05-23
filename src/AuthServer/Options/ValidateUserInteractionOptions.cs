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

        if (options.CreateAccountUri.IsNullOrEmpty() || !Uri.IsWellFormedUriString(options.CreateAccountUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.CreateAccountUri)} is not specified");
        }

        if (options.SelectAccountUri.IsNullOrEmpty() || !Uri.IsWellFormedUriString(options.SelectAccountUri, UriKind.Absolute))
        {
            return ValidateOptionsResult.Fail($"{nameof(options.SelectAccountUri)} is not specified");
        }

        return ValidateOptionsResult.Success;
    }
}