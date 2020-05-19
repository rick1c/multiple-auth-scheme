using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MultipleAuthSchemes.Pages
{
    [Authorize(AuthenticationSchemes = "OAuthTest")]
    public class Page2Model : PageModel
    {
        public void OnGet()
        {

        }
    }
}