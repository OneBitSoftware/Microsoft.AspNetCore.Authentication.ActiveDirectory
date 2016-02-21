namespace Microsoft.AspNetCore.Authentication.ActiveDirectory.Events
{
    using AspNet.Authentication;
    using Microsoft.AspNet.Http;

    public class AuthenticationSucceededContext : BaseActiveDirectoryContext
    {
        public AuthenticationSucceededContext(HttpContext context, ActiveDirectoryOptions options)
               : base(context, options)
        {
        }
    }
}
