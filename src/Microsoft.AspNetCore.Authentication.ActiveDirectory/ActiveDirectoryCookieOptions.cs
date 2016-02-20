using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    /// <summary>
    /// Represents all the options you can use to configure the cookies middleware.
    /// </summary>
    public class ActiveDirectoryCookieOptions
    {
        /// <summary>
        /// Default constructor for AD Cookie Options. Internally instantiates CookieAuthenticationOptions with default values.
        /// </summary>
        public ActiveDirectoryCookieOptions()
        {
            // Configure all of the cookie middlewares
            ApplicationCookie = new CookieAuthenticationOptions
            {
                AuthenticationScheme = ApplicationCookieAuthenticationScheme,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                ReturnUrlParameter = "ReturnUrl",
                LoginPath = new PathString("/windowsauthentication/ntlm"),
            };
        }

        /// <summary>
        /// Constructor accepting CookieAuthenticationOptions
        /// </summary>
        /// <param name="cookieOptions"></param>
        public ActiveDirectoryCookieOptions(CookieAuthenticationOptions cookieOptions)
        {
            // Configure all of the cookie middlewares
            ApplicationCookie = cookieOptions;
        }

        public CookieAuthenticationOptions ApplicationCookie { get; set; }

        /// <summary>
        /// Gets or sets the scheme used to identify application authentication cookies.
        /// </summary>
        /// <value>The scheme used to identify application authentication cookies.</value>
        public string ApplicationCookieAuthenticationScheme { get; set; } = typeof(ActiveDirectoryCookieOptions).Namespace + ".Application";

    }
}
