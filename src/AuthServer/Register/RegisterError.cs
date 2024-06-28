using AuthServer.Core;
using AuthServer.Core.Request;

namespace AuthServer.Register;
internal class RegisterError
{
	public static readonly ProcessError InvalidApplicationType = new(ErrorCode.InvalidClientMetadata,
		"invalid application_type", ResultCode.BadRequest);
}