namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using Extensions.WebEncoders;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
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


        /// <summary> 
        /// Check if the present request is actually a callpack path for the NTLM authentication middleware 
        /// </summary> 
        /// <param name="request"></param> 
        /// <param name="redirectPath">The path to check against</param> 
        /// <returns>True if the request path matches the callback path, false otherwise</returns> 
        public static bool IsNtlmAuthenticationCallback(
            this HttpRequest request,
            PathString redirectPath)
        {
            throw new NotImplementedException("check here");
            return (request.PathBase.Add(request.Path) == redirectPath);
        }

        /// <summary> 
        /// Check if the present request is actually a callpack path for the NTLM authentication middleware 
        /// </summary> 
        /// <remarks> 
        /// If you didn't use the default redirection path in the configuration of the NTLM authentication  
        /// middleware you must supply the same path to this function. See overloads of this method. 
        /// </remarks> 
        /// <param name="request"></param> 
        /// <returns>True if the request path is the callback path, false otherwise</returns> 
        public static bool IsNtlmAuthenticationCallback(
            this HttpRequest request)
        {
            return request.IsNtlmAuthenticationCallback(ActiveDirectoryOptions.DefaultRedirectPath);
        }
    }
}
