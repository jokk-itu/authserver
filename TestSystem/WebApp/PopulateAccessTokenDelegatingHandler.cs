using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http.Headers;

namespace WebApp;

public class PopulateAccessTokenDelegatingHandler : DelegatingHandler
{
  private readonly ILogger<PopulateAccessTokenDelegatingHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public PopulateAccessTokenDelegatingHandler(ILogger<PopulateAccessTokenDelegatingHandler> logger, IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    var accessToken = await _httpContextAccessor.HttpContext!.GetTokenAsync(OAuthDefaults.DisplayName, "access_token");
    if (accessToken is null)
    {
      _logger.LogWarning("Accesstoken is not present");
      return await base.SendAsync(request, cancellationToken);
    }
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    return await base.SendAsync(request, cancellationToken);
  }
}
