namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using Microsoft.AspNetCore.Builder;
    using System;
    using System.Text.Encodings.Web;

    /// <summary> 
    /// Extension methods for the ActiveDirectoryMiddleware
    /// </summary> 
    public static class ActiveDirectoryExtensions
    {
        /// <summary>
        /// Enables the NTLM route for challenge/response handshaking
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">An ActiveDirectoryOptions configuration</param>
        /// <returns></returns>
        public static IApplicationBuilder UseNtlm(this IApplicationBuilder app, ActiveDirectoryOptions options = null, UrlEncoder encoder = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            //set default encoder if none is provided
            if (encoder == null) encoder = UrlEncoder.Default;

            if (options != null)
            {
                return app.UseMiddleware<ActiveDirectoryMiddleware>(options, encoder);
            }

            return app.UseMiddleware<ActiveDirectoryMiddleware>(encoder);
        }

        public static IApplicationBuilder UseKerberos(this IApplicationBuilder app, ActiveDirectoryOptions options = null)
        {
            throw new NotImplementedException("Kerberos support is not yet ready :(");
        }

        public static IApplicationBuilder UseWindowsIntegratedAuthentication(this IApplicationBuilder app, ActiveDirectoryOptions options = null)
        {
            throw new NotImplementedException("Windows Integrated Authentication support is not yet ready :(");
        }
    }
}
