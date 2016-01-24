using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNet.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    public class NtlmAuthenticationHandler : AuthenticationHandler<ActiveDirectoryOptions>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NtlmAuthenticationHandler()
        {
        }

        /// <summary>
        /// This is always invoked on each request. For passive middleware, only do anything if this is
        /// for our callback path when the user is redirected back from the authentication provider.
        /// </summary>
        /// <returns></returns>
        public async override Task<bool> HandleRequestAsync()
        {
            //if (Options.CallbackPath.HasValue && Options.CallbackPath ==
            //    Request.PathBase.Add(Request.Path))
            //{
            //    var authContext = new AuthenticateContext(Options.AuthenticationScheme);
            //    await AuthenticateAsync(authContext);
            //    if (authContext.Accepted && authContext.Principal != null)
            //    {
            //        await Context.Authentication.SignInAsync(authContext.AuthenticationScheme,
            //            authContext.Principal);

            //        //return user to page
            //        var responseUri = string.Empty;
            //        authContext.Properties.TryGetValue("RedirectUri", out responseUri);
            //        if (string.IsNullOrWhiteSpace(responseUri))
            //            Response.Redirect("/");
            //        else
            //            Response.Redirect(responseUri);

            //        return true;
            //    }
            //    if (Response.Headers.ContainsKey("WWW-Authenticate"))
            //    {
            //        return true;
            //    }
            //}
            return await base.HandleRequestAsync();
        }

        #region Helpers
        protected override Task FinishResponseAsync()
        {
            //We need to fix the issues that the CookieAuthenticationHandler is leaving behind
            //CookieAuthenticationHandler doesn't work well with NTLM handshaking
            //but we need it to retain the session and remove unnecessary handshaking
            if (Response.StatusCode == 302)
            {
                if (Context.Items.ContainsKey("NtlmAuthenticationHandlerRespondNoNtlm") ||
                    Context.Items.ContainsKey("NtlmAuthenticationHandlerRespondType2"))
                {
                    //we're cleaning up the location set by CookieAuthenticationHandler.HandleUnauthorizedAsync
                    Response.Headers.Remove("Location"); 
                    Response.StatusCode = 401;
                }

                if ((Context.Items.ContainsKey("NtlmAuthenticationHandlerAuthenticated"))
                    && Request.Query.ContainsKey(Options.Cookies.ApplicationCookie.ReturnUrlParameter))
                {
                    //we're cleaning up the location set by CookieAuthenticationHandler.HandleUnauthorizedAsync
                    Response.Headers["Location"] = Request.Query[Options.Cookies.ApplicationCookie.ReturnUrlParameter];
                }
            }

            return base.FinishResponseAsync();
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //if accessed from a different URL - ignore
            if (!Request.Path.Equals(Options.CallbackPath))
            {
                Context.Items["NtlmAuthenticationHandlerRedirectToEndpoint"] = true;
                Response.StatusCode = 302;
                return AuthenticateResult.Failed("Redirecting to authentication route /windowsauthentication/ntlm");
            }

            //check if the request has an NTLM header
            var authorizationHeader = Request.Headers["Authorization"];

            byte[] token = null;
            var hasNtlm = authorizationHeader.Any(h => h.StartsWith("NTLM "));
            if (!hasNtlm)
            {
                // This code runs under following conditions:
                // - authentication failed (in either step: IsClientResponseValid() or TryAcquireServerChallenge())
                // - there's no token in the headers
                //
                // This means we've got to set the WWW-Authenticate header and return a 401
                // 401 tells the browser that the request is unauthenticated and the WWW-Authenticate
                // header tells the browser to try again with NTLM
                Response.Headers.Add("WWW-Authenticate", new[] { "NTLM" });
                Response.StatusCode = 401;
                Context.Items["NtlmAuthenticationHandlerRespondNoNtlm"] = true;

                return AuthenticateResult.Failed("No NTLM header, returning WWW-Authenticate NTLM.");
            }

            if (!string.IsNullOrEmpty(authorizationHeader) && hasNtlm)
            {
                var header = authorizationHeader.First(h => h.StartsWith("NTLM "));
                token = Convert.FromBase64String(header.Substring(5));
            }

            var stateId = Request.Query["state"];
            if (string.IsNullOrWhiteSpace(stateId)) stateId = "123";
            HandshakeState state = null;// new HandshakeState();
            this.Options.LoginStateCache.TryGet(stateId, out state);

            if (state == null) state = new HandshakeState();

            // First eight bytes are header containing NTLMSSP\0 signature
            // Next byte contains type of the message recieved.
            // No Token - it's the initial request. Add a authenticate header
            // Message Type 1 — is initial client's response to server's 401 Unauthorized error.
            // Message Type 2 — is the server's response to it. Contains random 8 bytes challenge.
            // Message Type 3 — is encrypted password hashes from client ready to server validation.
            if (token != null && token[8] == 1)
            {
                // Message of type 1 was received
                if (state.TryAcquireServerChallenge(ref token))
                {
                    // send the type 2 message
                    var authorization = Convert.ToBase64String(token);
                    Response.Headers.Add("WWW-Authenticate", new[] { string.Concat("NTLM ", authorization) });
                    Response.StatusCode = 401;

                    Options.LoginStateCache.Add("123", state);
                    Context.Items["NtlmAuthenticationHandlerRespondType2"] = true;

                    return AuthenticateResult.Failed("Received NTLM Type 1, sending Type 2 with status 401.");
                }
            }
            else if (token != null && token[8] == 3)
            {
                // message of type 3 was received
                if (state.IsClientResponseValid(token))
                {
                    // Authorization successful 
                    var properties = state.AuthenticationProperties;

                    if (Options.Filter == null || Options.Filter.Invoke(state.WindowsIdentity, Request))
                    {
                        // If the name is something like DOMAIN\username then
                        // grab the name part (and what if it looks like username@domain?)
                        var parts = state.WindowsIdentity.Name.Split(new[] { '\\' }, 2);
                        string shortName = parts.Length == 1 ? parts[0] : parts[parts.Length - 1];

                        // we need to create a new identity using the sign in type that 
                        // the cookie authentication is listening for
                        var identity = new ClaimsIdentity(Options.Cookies.ApplicationCookie.AuthenticationScheme);

                        identity.AddClaims(new[]
                        {
                                new Claim(ClaimTypes.NameIdentifier,
                                state.WindowsIdentity.User.Value, null,
                                Options.AuthenticationScheme),
                                new Claim(ClaimTypes.Name, shortName),
                                new Claim(ClaimTypes.Sid, state.WindowsIdentity.User.Value),
                                new Claim(ClaimTypes.AuthenticationMethod,
                                ActiveDirectoryOptions.DefaultAuthenticationScheme)
                            });

                        // We don't need that state anymore
                        Options.LoginStateCache.TryRemove(stateId);

                        // create the authentication ticket
                        var principal = new ClaimsPrincipal(identity);
                        var ticket = new AuthenticationTicket(principal, properties, Options.Cookies.ApplicationCookie.AuthenticationScheme);

                        await Context.Authentication.SignInAsync(Options.Cookies.ApplicationCookie.AuthenticationScheme, principal, properties);
                        Context.Items["NtlmAuthenticationHandlerAuthenticated"] = true;

                        return AuthenticateResult.Success(ticket);
                    }
                }
            }

            return AuthenticateResult.Failed("Unauthorized");
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (Response.StatusCode != 302)
                await base.HandleUnauthorizedAsync(context);

            return true;
        }

        protected override async Task HandleSignInAsync(SignInContext context)
        {
            await base.HandleSignInAsync(context);
            SignInAccepted = true;
        }
        protected override Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            return base.HandleForbiddenAsync(context);
        }
        #endregion
    }

}
