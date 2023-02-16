using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;

namespace HybridApp;
public class AuthenticationBrowser : IdentityModel.OidcClient.Browser.IBrowser
{
  public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = new CancellationToken())
  {
    try
    {
      var result = await WebAuthenticator.Default.AuthenticateAsync(
        new Uri(options.StartUrl),
        new Uri(options.EndUrl));

      var url = new RequestUrl("hybridapp://callback")
        .Create(new Parameters(result.Properties));

      return new BrowserResult
      {
        Response = url,
        ResultType = BrowserResultType.Success
      };
    }
    catch (TaskCanceledException)
    {
      return new BrowserResult
      {
        ResultType = BrowserResultType.UserCancel
      };
    }
    catch (Exception)
    {
      return new BrowserResult
      {
        ResultType = BrowserResultType.UnknownError
      };
    }
  }
}