using AuthServer.Core;
using AuthServer.Core.RequestProcessing;

namespace AuthServer.Register;
internal class PostRegisterError
{
	public static readonly ProcessError InvalidApplicationType = new(ErrorCode.InvalidClientMetadata,
		"invalid application_type", ResultCode.BadRequest);
}