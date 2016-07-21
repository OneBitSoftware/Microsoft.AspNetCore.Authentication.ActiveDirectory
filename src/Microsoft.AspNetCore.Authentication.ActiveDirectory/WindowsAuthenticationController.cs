namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class WindowsAuthenticationController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Ntlm(string returnUrl = null)
        {
            if (this.User.Identity.IsAuthenticated == false)
            {
                var defaultProperties = new AuthenticationProperties() { RedirectUri = returnUrl };

                var context = this.Request.HttpContext;
                await context.Authentication.ChallengeAsync(ActiveDirectoryOptions.DefaultAuthenticationScheme, defaultProperties);

                if (context.Response.StatusCode == 302)
                    return new StatusCodeResult(302);
                else {
                    return new UnauthorizedResult();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                    return new OkResult();
                else
                    return Redirect(returnUrl);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> LogOut(string returnUrl = null)
        {
            var context = this.Request.HttpContext;
            await context.Authentication.SignOutAsync(ActiveDirectoryOptions.DefaultAuthenticationScheme);
            if (string.IsNullOrWhiteSpace(returnUrl))
                return new OkResult();
            else
                return Redirect(returnUrl);
        }
    }
}
