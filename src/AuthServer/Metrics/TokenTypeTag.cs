using System.ComponentModel;

namespace AuthServer.Metrics;
internal enum TokenTypeTag
{
    [Description("access_token")]
    AccessToken,

    [Description("refresh_token")]
    RefreshToken,

    [Description("id_token")]
    IdToken,

    [Description("logout_token")]
    LogoutToken,

    [Description("userinfo_token")]
    UserinfoToken,

    [Description("registration_token")]
    RegistrationToken,

    [Description("client_assertion")]
    ClientAssertion,

    [Description("request_object")]
    RequestObject,
}
