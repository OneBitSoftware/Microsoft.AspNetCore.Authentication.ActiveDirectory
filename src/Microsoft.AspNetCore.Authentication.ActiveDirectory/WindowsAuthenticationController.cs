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

                var authContext = new Http.Features.Authentication.AuthenticateContext(ActiveDirectoryOptions.DefaultAuthenticationScheme);
                await HttpContext.Authentication.AuthenticateAsync(authContext);

                if (!authContext.Accepted || authContext.Principal == null)
                {
                    return new UnauthorizedResult();
                }
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
                return new OkResult();
            else
                return Redirect(returnUrl);
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
