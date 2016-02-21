using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory.Events
{
    public class AuthenticationEvents : IAuthenticationEvents
    {
        /// <summary> 
        /// Invoked when the Active Directory authentication process has succeeded and authenticated the user. 
        /// </summary> 
        public Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.FromResult(0);

        /// <summary> 
        /// Invoked when the authentication handshaking failed and the user is not authenticated.
        /// </summary> 
        public Func<AuthenticationSucceededContext, Task> OnAuthenticationSucceeded { get; set; } = context => Task.FromResult(0);

        public virtual Task AuthenticationFailed(AuthenticationFailedContext context)
            => OnAuthenticationFailed(context);

        public virtual Task AuthenticationSucceeded(AuthenticationSucceededContext context)
            => OnAuthenticationSucceeded(context);
    }
}
