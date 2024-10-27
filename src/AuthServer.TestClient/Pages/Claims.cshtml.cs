using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.TestClient.Pages;

[Authorize]
public class ClaimsModel : PageModel
{
    public void OnGet()
    {
    }
}