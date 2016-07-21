namespace Microsoft.AspNetCore.Authentication.ActiveDirectory.Events
{
    using AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public class AuthenticationSucceededContext : BaseActiveDirectoryContext
    {
        public AuthenticationSucceededContext(HttpContext context, ActiveDirectoryOptions options)
               : base(context, options)
        {
        }
    }
}
