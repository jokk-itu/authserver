using AuthServer.Core.Abstractions;
using AuthServer.Core.Request;
using AuthServer.Extensions;
using AuthServer.RequestAccessors.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Endpoints;

internal static class DeleteRegisterEndpoint
{
	public static async Task<IResult> HandleDeleteRegister(
		HttpContext httpContext,
		[FromServices] IRequestAccessor<DeleteRegisterRequest> requestAccessor,
		[FromServices] IRequestHandler<DeleteRegisterRequest, Unit> requestHandler,
		CancellationToken cancellationToken)
	{
		var request = await requestAccessor.GetRequest(httpContext.Request);
		var response = await requestHandler.Handle(request, cancellationToken);
		return response.Match(
			_ => Results.NoContent(),
			error => Results.Extensions.OAuthBadRequest(error));
	}
}