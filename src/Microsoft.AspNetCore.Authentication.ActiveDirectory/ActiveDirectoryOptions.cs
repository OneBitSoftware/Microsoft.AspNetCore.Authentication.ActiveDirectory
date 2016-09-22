namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Authentication;
    using System.Security.Principal;
    using Events;
    using Extensions.Options;

    public class ActiveDirectoryOptions : AuthenticationOptions, IOptions<ActiveDirectoryOptions>
    {
        public const string DefaultAuthenticationScheme = "Ntlm";

        #region Internal fields
        /// <summary>
        /// The default redirection path used by the NTLM authentication middleware of
        /// the full roundtrip / handshakes
        /// </summary>
        internal static readonly PathString DefaultRedirectPath = new PathString("/windowsauthentication/ntlm");

        /// <summary>
        /// Secured store for state data
        /// </summary>
        internal ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// Store states for the login attempts
        /// </summary>
        internal AuthenticationStateCache LoginStateCache { get; set; }
        #endregion

        /// <summary>
        /// Number of minutes a login can take (defaults to 2 minutes)
        /// </summary>
        public int LoginStateExpirationTime
        {
            set { LoginStateCache.ExpirationTime = value; }
            get { return LoginStateCache.ExpirationTime; }
        }

        /// <summary>
        /// The authentication scheme used for sign in
        /// </summary>
        public string SignInAsAuthenticationScheme { get; set; }

        /// <summary>
        /// The callback string used for the NTLM authentication roundtrips, 
        /// defaults to "/authentication/ntlm-signin"
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// If this is set, it must return true to authenticate the user.
        /// It can be used to filter out users according to separate criteria.
        /// </summary>
        /// <remarks>
        /// Note that the Windows identity will be disposed shortly after this function has returned
        /// </remarks>
        public Func<WindowsIdentity, HttpRequest, bool> Filter { get; set; } //TODO: httpRquest?

        /// <summary>
        /// Creates an instance of Ntlm authentication options with default values.
        /// </summary>
        public ActiveDirectoryOptions()
        {
            this.SignInAsAuthenticationScheme = DefaultAuthenticationScheme;
            this.CallbackPath = DefaultRedirectPath;
            this.LoginStateCache = new AuthenticationStateCache();
            this.LoginStateExpirationTime = 5;
        }

        public ActiveDirectoryCookieOptions Cookies { get; set; } = new ActiveDirectoryCookieOptions();
         
        /// <summary> 
        /// The object provided by the application to process events raised by the Active Directory authentication middleware. 
        /// The application may implement the interface fully, or it may create an instance of AuthenticationEvents 
        /// and assign delegates only to the events it wants to process. 
        /// </summary> 
        public IAuthenticationEvents Events { get; set; } = new AuthenticationEvents();

        public ActiveDirectoryOptions Value
        {
            get
            {
                return this;
            }
        }
    }
}
