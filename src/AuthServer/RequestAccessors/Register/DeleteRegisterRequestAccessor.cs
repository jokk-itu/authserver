using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Register;

internal class DeleteRegisterRequestAccessor : IRequestAccessor<DeleteRegisterRequest>
{
	public async Task<DeleteRegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var clientId = httpRequest.Query.GetValueOrEmpty(Parameter.ClientId);
		var registrationAccessToken = await httpRequest.HttpContext.GetTokenAsync(Parameter.AccessToken) ?? string.Empty;

		return new DeleteRegisterRequest
		{
			ClientId = clientId,
			RegistrationAccessToken = registrationAccessToken,
		};
	}
}