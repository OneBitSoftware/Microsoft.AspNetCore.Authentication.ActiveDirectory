using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNetCore.Authentication.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample_AspNet5.Mvc6.Ntlm.Controllers
{
    public class WindowsAuthenticationController : Controller
    {
        [AllowAnonymous]
        [Route("/windowsauthentication/ntlm")]
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
