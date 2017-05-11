# Microsoft.AspNetCore.Authentication.ActiveDirectory
NOTE: AppVeyor build is failing due to build scripts not yet updated. Solution is OK. [![Build status](https://ci.appveyor.com/api/projects/status/hhd468o15oct73sg?svg=true)](https://ci.appveyor.com/project/SharePointRadi/microsoft-aspnetcore-authentication-activedirector)

Middleware for ASP.NET Core for Windows Integrated Authentication with NTLM and Kerberos

## Overview
This ASP.NET Core middleware lets you authenticate to Active Directory. 

The old school ASP.NET Membership capabilities and Forms Authentication had a nice LDAP provider, and IIS has native Windows Integrated Authentication capability, supporting both NTLM and Kerberos authentication. 

ASP.NET Core doesn't have NTLM/Kerberos authentication middleware and ASP.NET Identity 3 doesn't have an LDAP provider. Usually, IIS handles this (and it still can), but what if you are hosting on Kestrel? This library allows you to do Windows Integrated Authentication with ASP.NET Core.

## Status
NTLM is working. Kerberos is not attempted yet. 

Todo:
- Get some unit tests in place
- Implement Kerberos

## Getting Started
1. Review the sample in the `samples` folder.
2. Either install through the NuGet package: https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.ActiveDirectory/

	OR just reference the source code directly.
3. Add the `Microsoft.AspNetCore.Authentication.ActiveDirectory` library to your project.json file
4. Add the following line `services.AddAuthentication(options => new ActiveDirectoryCookieOptions());` in your Startup.cs `ConfigureServices` method
5. Add `app.UseCookieAuthentication(new ActiveDirectoryCookieOptions().ApplicationCookie);` to your Startup.cs `Configure` method
6. Add the following after the `app.UseCookieAuthentication()` line (Step 4):

```cs            
app.UseNtlm(new ActiveDirectoryOptions
{
    AutomaticAuthenticate = false,
    AutomaticChallenge = false,
    AuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
    SignInAsAuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
});
```
At this stage your middleware is accessible through the {site}/windowsauthentication/ntlm endpoint. ReturnUrl will take the user to the page after login. I use this endpoint to perform NTLM handshaking.

## Setting up a custom controller URL
If you don't like the default "/windowsauthentication/ntlm", you can use the CallbackPath and LoginPath settings to configure your alternative route.

```cs
//ActiveDirectory: set up cookies for client-side session identitfication
app.UseCookieAuthentication(
    new ActiveDirectoryCookieOptions(
        new CookieAuthenticationOptions()
        {
            AuthenticationScheme = typeof(ActiveDirectoryCookieOptions).Namespace + ".Application",
            AutomaticAuthenticate = true,
            AutomaticChallenge = true,
            ReturnUrlParameter = "ReturnUrl",
            LoginPath = new PathString("/api/windowsauthentication/ntlm"),
        }).ApplicationCookie
);

//ActiveDirectory: add the NTLM middlware in the pipeline
app.UseNtlm(new ActiveDirectoryOptions
{
    AutomaticAuthenticate = false,
    AutomaticChallenge = false,
    AuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
    SignInAsAuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
    CallbackPath = new PathString("/api/windowsauthentication/ntlm")
});
```
You will need to make sure MVC can resolve the route in UseMvc():

```cs
routes.MapRoute(
    name: "authentication",
    template: "api/{controller=WindowsAuthentication}/{action=Ntlm}");
```

## Events
If you choose to do so, you can subscribe to authentication events thrown from the middleware. To do so, pass an AuthenticationEvents class to the ActiveDirectoryOptions object during startup:

```cs
app.UseNtlm(new ActiveDirectoryOptions
{
    AutomaticAuthenticate = false,
    AutomaticChallenge = false,
    AuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
    SignInAsAuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,

    //Optionally, you can handle the events below
    Events = new AuthenticationEvents()
    {
        OnAuthenticationSucceeded = succeededContext =>
        {
            var userName = succeededContext.AuthenticationTicket.Principal.Identity.Name;

            //do something on successful authentication

            return Task.FromResult<object>(null);
        },
        OnAuthenticationFailed = failedContext =>
        {
            //do something on failed authentication

            return Task.FromResult<object>(null);
        }
    }
});
```

## Remarks
Achieving server-side NTLM handshaking depends the Windows platform due to interop, dependency on domain-joined machines and secur32.dll. This will naturally limit this library to Windows-based DNX hosts.

See https://tools.ietf.org/html/rfc4559 for more info on NTLM

## Kudos
Most of the code here is based on what Yannic Staudt developed here: https://github.com/pysco68/Pysco68.Owin.Authentication.Ntlm
It is adapted for ASP.NET Core with some changes to the logic. A HUGE thanks for the interop class!

##Contribution
Feel free to reach out, I would love to hear if you are using this (or trying to). Pull requests are more than welcome.
