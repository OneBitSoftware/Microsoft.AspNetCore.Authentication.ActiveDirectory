using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    public class NtlmAuthenticationHandler : AuthenticationHandler<ActiveDirectoryOptions>
    {
        private const string RedirectToEndpointKey = "NtlmAuthenticationHandlerRedirectToEndpoint";
        private const string RespondNoNtlmKey = "NtlmAuthenticationHandlerRespondNoNtlm";
        private const string RespondType2Key = "NtlmAuthenticationHandlerRespondType2";
        private const string AuthenticatedKey = "NtlmAuthenticationHandlerAuthenticated";
        private const string LocationKey = "LocationKey";
        private const string NtlmAuthUniqueIdCookieKey = "NtlmAuthUniqueId";

        protected override Task FinishResponseAsync()
        {
            //We need to fix the issues that the CookieAuthenticationHandler is leaving behind
            //CookieAuthenticationHandler doesn't work well with NTLM handshaking
            //but we need it to retain the session and remove unnecessary handshaking
            if (Response.StatusCode == 302)
            {
                if (Context.Items.ContainsKey(RespondNoNtlmKey) ||
                    Context.Items.ContainsKey(RespondType2Key))
                {
                    //we're cleaning up the location set by CookieAuthenticationHandler.HandleUnauthorizedAsync
                    Response.Headers.Remove(LocationKey);
                    Response.StatusCode = 401;
                }

                if ((Context.Items.ContainsKey(AuthenticatedKey))
                    && Request.Query.ContainsKey(Options.Cookies.ApplicationCookie.ReturnUrlParameter))
                {
                    //we're cleaning up the location set by CookieAuthenticationHandler.HandleUnauthorizedAsync
                    Response.Redirect(Request.Query[Options.Cookies.ApplicationCookie.ReturnUrlParameter]);
                }
            }

            //The following prevents the Cookie auth middleware to set the response to 403 Forbidden
            if ((Response.StatusCode == 401) &&
                (Context.Items.ContainsKey(RespondNoNtlmKey)) ||
                (Context.Items.ContainsKey(RespondType2Key)))
            {
                if (PriorHandler.GetType().FullName == "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationHandler")
                {
                    var challengeContext = new ChallengeContext(ActiveDirectoryOptions.DefaultAuthenticationScheme);
                    PriorHandler.ChallengeAsync(challengeContext);
                }
            }

            return base.FinishResponseAsync();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //if accessed from a different URL - ignore
            if (!Request.Path.Equals(Options.CallbackPath))
            {
                Context.Items[RedirectToEndpointKey] = true;
                Response.StatusCode = 302;
                return AuthenticateResult.Fail("Redirecting to authentication route /windowsauthentication/ntlm");
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
                Context.Items[RespondNoNtlmKey] = true;

                //We're creating a unique guid to identify the client between the
                //Type 2 and Type 3 handshake
                var requestUniqueId = Guid.NewGuid();
                Response.Cookies.Append(NtlmAuthUniqueIdCookieKey, requestUniqueId.ToString());

                return AuthenticateResult.Fail("No NTLM header, returning WWW-Authenticate NTLM.");
            }

            if (!string.IsNullOrEmpty(authorizationHeader) && hasNtlm)
            {
                var header = authorizationHeader.First(h => h.StartsWith("NTLM "));
                token = Convert.FromBase64String(header.Substring(5));
            }

            var responseUniqueId = Request.Cookies[NtlmAuthUniqueIdCookieKey];
            HandshakeState state = null;
            //see if the response is from a known client handshake
            if (!string.IsNullOrWhiteSpace(responseUniqueId))
            {
                this.Options.LoginStateCache.TryGet(responseUniqueId, out state);
            }

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

                    Options.LoginStateCache.Add(responseUniqueId, state);
                    Context.Items[RespondType2Key] = true;

                    return AuthenticateResult.Fail("Received NTLM Type 1, sending Type 2 with status 401.");
                }
            }
            else if (token != null && token[8] == 3)
            {
                // message of type 3 was received, we validate it
                if (state.IsClientResponseValid(token))
                {
                    // Authorization successful 
                    var properties = state.AuthenticationProperties;

                    if (Options.Filter == null || Options.Filter.Invoke(state.WindowsIdentity, Request))
                    {
                        AuthenticationTicket ticket = await CreateAuthenticationTicket(state.WindowsIdentity.Claims, properties);

                        // We don't need that state anymore
                        Options.LoginStateCache.TryRemove(responseUniqueId);

                        //throw the succeded event
                        await Options.Events.AuthenticationSucceeded(new Events.AuthenticationSucceededContext(Context, Options)
                        {
                            Ticket = ticket //pass the ticket
                        });

                        return AuthenticateResult.Success(ticket);
                    }
                }
            }

            await Options.Events.AuthenticationFailed(new Events.AuthenticationFailedContext(Context, Options));

            return AuthenticateResult.Fail("Unauthorized");
        }

        private async Task<AuthenticationTicket> CreateAuthenticationTicket(IEnumerable<Claim> claims, AuthenticationProperties properties)
        {
            // we need to create a new identity using the sign in type that 
            // the cookie authentication is listening for
            var identity = new ClaimsIdentity(Options.Cookies.ApplicationCookie.AuthenticationScheme);

            //Add WindowsIdentity claims to the Identity object
            identity.AddClaims(claims);

            // create the authentication ticket
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket
                (principal, properties,
                Options.Cookies.ApplicationCookie.AuthenticationScheme);

            //handle the sign in method of the auth middleware
            await Context.Authentication.SignInAsync
                (Options.Cookies.ApplicationCookie.AuthenticationScheme,
                principal, properties);

            Context.Items[AuthenticatedKey] = true;
            return ticket;
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (Response.StatusCode != 302)
                await base.HandleUnauthorizedAsync(context);

            return true;
        }
        
        public override async Task<bool> HandleRequestAsync()
        {
            //var authContext = new AuthenticateContext(Options.Cookies.ApplicationCookie.AuthenticationScheme);
            //await this.Context.Authentication.AuthenticateAsync(authContext);
            return await base.HandleRequestAsync();
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

        protected override async Task HandleSignOutAsync(SignOutContext context)
        {
            await Context.Authentication.SignOutAsync(Options.Cookies.ApplicationCookie.AuthenticationScheme);
            await base.HandleSignOutAsync(context);
        }
    }
}