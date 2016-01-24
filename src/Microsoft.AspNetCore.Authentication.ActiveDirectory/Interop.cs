using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    class Interop
    {
        /// <summary>
        /// The AcquireCredentialsHandle function acquires a handle to preexisting credentials of a security principal.
        /// </summary>
        [DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int AcquireCredentialsHandle(
            string pszPrincipal,
            string pszPackage,
            int fCredentialUse,
            IntPtr PAuthenticationID,
            IntPtr pAuthData,
            int pGetKeyFn,
            IntPtr pvGetKeyArgument,
            ref SecurityHandle phCredential,
            ref SecurityInteger ptsExpiry);

        /// <summary>
        /// The AcceptSecurityContext (General) function enables the server component of a 
        /// transport application to establish a security context between the server and a remote client.
        /// </summary>
        [DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int AcceptSecurityContext(ref SecurityHandle phCredential,
            IntPtr phContext,
            ref SecurityBufferDesciption pInput,
            uint fContextReq,
            uint TargetDataRep,
            out SecurityHandle phNewContext,
            out SecurityBufferDesciption pOutput,
            out uint pfContextAttr,
            out SecurityInteger ptsTimeStamp);

        /// <summary>
        /// The AcceptSecurityContext (General) function enables the server component of a 
        /// transport application to establish a security context between the server and a remote client.
        /// </summary>
        [DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        public static extern int AcceptSecurityContext(ref SecurityHandle phCredential,
            ref SecurityHandle phContext,
            ref SecurityBufferDesciption pInput,
            uint fContextReq,
            uint TargetDataRep,
            out SecurityHandle phNewContext,
            out SecurityBufferDesciption pOutput,
            out uint pfContextAttr,
            out SecurityInteger ptsTimeStamp);

        /// <summary>
        /// Obtains the access token for a client security context and uses it directly.
        /// </summary>
        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int QuerySecurityContextToken(
            ref SecurityHandle phContext,
            ref IntPtr phToken);

        /// <summary>
        /// Close handle for proper cleanup
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
    }

}
