namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Mvc;
    using System.Threading.Tasks;

    public class WindowsAuthenticationController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Ntlm(string returnUrl)
        {
            if (this.User.Identity.IsAuthenticated == false)
            {
                var defaultProperties = new AuthenticationProperties() { RedirectUri = returnUrl };

                var context = this.Request.HttpContext;
                await context.Authentication.ChallengeAsync(ActiveDirectoryOptions.DefaultAuthenticationScheme, defaultProperties);

                if (context.Response.StatusCode == 302)
                    return new HttpStatusCodeResult(302);
                else {
                    return new HttpUnauthorizedResult();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                    return Redirect("/");
                else
                    return Redirect(returnUrl);
            }
        }
    }
}
