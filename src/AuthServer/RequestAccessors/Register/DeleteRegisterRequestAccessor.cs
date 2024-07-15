using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Register;

internal class DeleteRegisterRequestAccessor : IRequestAccessor<DeleteRegisterRequest>
{
	public async Task<DeleteRegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var clientId = httpRequest.Query.GetValueOrEmpty(Parameter.ClientId);
        var registrationAccessToken = await httpRequest.HttpContext.GetToken(Parameter.AccessToken);

		return new DeleteRegisterRequest
		{
			ClientId = clientId,
			RegistrationAccessToken = registrationAccessToken,
		};
	}
}