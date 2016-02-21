using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory.Events
{
    public interface IAuthenticationEvents
    {
         /// <summary> 
         /// Invoked when the Active Directory authentication process has succeeded and authenticated the user. 
         /// </summary> 
         Task AuthenticationSucceeded(AuthenticationSucceededContext context); 
 

         /// <summary> 
         /// Invoked when the authentication handshaking failed and the user is not authenticated.
         /// </summary> 
         Task AuthenticationFailed(AuthenticationFailedContext context);
    }
}
