using AuthServer.Core;
using AuthServer.Core.Abstractions;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace AuthServer.RequestAccessors.Register;

internal class GetRegisterRequestAccessor : IRequestAccessor<GetRegisterRequest>
{
	public async Task<GetRegisterRequest> GetRequest(HttpRequest httpRequest)
	{
		var clientId = httpRequest.Query.GetValueOrEmpty(Parameter.ClientId);
		var registrationAccessToken = await httpRequest.HttpContext.GetTokenAsync(Parameter.AccessToken) ?? string.Empty;

		return new GetRegisterRequest
		{
			ClientId = clientId,
			RegistrationAccessToken = registrationAccessToken,
		};
	}
}