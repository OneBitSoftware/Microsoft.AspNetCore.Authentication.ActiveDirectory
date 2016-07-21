namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using Microsoft.AspNetCore.Http.Authentication;
    using System;
    using System.Security.Principal;

    /// <summary>
    /// A windows authentication session
    /// </summary>
    public class HandshakeState : IDisposable
    {
        public HandshakeState()
        {
            this.Credentials = new SecurityHandle(0);
            this.Context = new SecurityHandle(0);
        }

        /// <summary>
        /// Credentials used to validate NTLM hashes
        /// </summary>
        private SecurityHandle Credentials;

        /// <summary>
        /// Context will be used to validate HTLM hashes
        /// </summary>
        private SecurityHandle Context;

        /// <summary>
        /// The authentication properties we extract from the authentication challenge
        /// received from application layer
        /// </summary>
        public AuthenticationProperties AuthenticationProperties;

        /// <summary>
        /// The matching windows identity
        /// </summary>
        public WindowsIdentity WindowsIdentity { get; set; }

        /// <summary>
        /// Try to acquire the server challenge for this state
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool TryAcquireServerChallenge(ref byte[] message)
        {
            SecurityBufferDesciption clientToken = new SecurityBufferDesciption(message);
            SecurityBufferDesciption serverToken = new SecurityBufferDesciption(Constants.MaximumTokenSize);

            try
            {
                int result;
                var lifetime = new SecurityInteger(0);

                result = Interop.AcquireCredentialsHandle(
                    null,
                    "NTLM",
                    Constants.SecurityCredentialsInbound,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    ref this.Credentials,
                    ref lifetime);

                if (result != Constants.SuccessfulResult)
                {
                    // Credentials acquire operation failed.
                    return false;
                }

                uint contextAttributes;

                result = Interop.AcceptSecurityContext(
                    ref this.Credentials,                       // [in] handle to the credentials
                    IntPtr.Zero,                                // [in/out] handle of partially formed context.  Always NULL the first time through
                    ref clientToken,                            // [in] pointer to the input buffers
                    Constants.StandardContextAttributes,           // [in] required context attributes
                    Constants.SecurityNativeDataRepresentation,    // [in] data representation on the target
                    out this.Context,                           // [in/out] receives the new context handle    
                    out serverToken,                            // [in/out] pointer to the output buffers
                    out contextAttributes,                      // [out] receives the context attributes        
                    out lifetime);                              // [out] receives the life span of the security context

                if (result != Constants.IntermediateResult)
                {
                    // Client challenge issue operation failed.
                    return false;
                }
            }
            finally
            {
                message = serverToken.GetBytes();
                clientToken.Dispose();
                serverToken.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Validate the client response and fill the indentity of the token
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsClientResponseValid(byte[] message)
        {
            SecurityBufferDesciption clientToken = new SecurityBufferDesciption(message);
            SecurityBufferDesciption serverToken = new SecurityBufferDesciption(Constants.MaximumTokenSize);
            IntPtr securityContextHandle = IntPtr.Zero;

            try
            {
                int result;
                uint contextAttributes;
                var lifetime = new SecurityInteger(0);

                result = Interop.AcceptSecurityContext(
                    ref this.Credentials,                       // [in] handle to the credentials
                    ref this.Context,                           // [in/out] handle of partially formed context.  Always NULL the first time through
                    ref clientToken,                            // [in] pointer to the input buffers
                    Constants.StandardContextAttributes,           // [in] required context attributes
                    Constants.SecurityNativeDataRepresentation,    // [in] data representation on the target
                    out this.Context,                           // [in/out] receives the new context handle    
                    out serverToken,                            // [in/out] pointer to the output buffers
                    out contextAttributes,                      // [out] receives the context attributes        
                    out lifetime);                              // [out] receives the life span of the security context

                if (result != Constants.SuccessfulResult)
                {
                    return false;
                }

                if (Interop.QuerySecurityContextToken(ref this.Context, ref securityContextHandle) != 0)
                {
                    return false;
                }

                var identity = new WindowsIdentity(securityContextHandle);

                if (identity == null)
                {
                    return false;
                }

                this.WindowsIdentity = identity;

            }
            finally
            {
                clientToken.Dispose();
                serverToken.Dispose();

                Interop.CloseHandle(securityContextHandle);

                this.Credentials.Reset();
                this.Context.Reset();
            }

            return true;
        }

        public void Dispose()
        {
            this.Context.Reset();
            this.Credentials.Reset();
            this.WindowsIdentity.Dispose();
        }
    }

}
