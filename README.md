# Microsoft.AspNetCore.Authentication.ActiveDirectory
[![Build status](https://ci.appveyor.com/api/projects/status/hhd468o15oct73sg?svg=true)](https://ci.appveyor.com/project/SharePointRadi/microsoft-aspnetcore-authentication-activedirector)

Middleware for ASP.NET 5 for Windows Integrated Authentication with NTLM and Kerberos

##Overview
This ASP.NET 5 middleware lets you authenticate to Active Directory. 

The old school ASP.NET Membership capabilities and Forms Authentication had a nice LDAP provider, and IIS has native Windows Integrated Authentication capability, supporting both NTLM and Kerberos authentication. 

The new ASP.NET 5 stuff doesn't have NTLM/Kerberos authentication middleware and ASP.NET Identity 3 doesn't have an LDAP provider. This library allows you to do Windows Integrated Authentication with ASP.NET 5.

##Status
This is still work in progress. Kerberos is not attempted yet. 

Todo:
- Create a Log Out action link
- Get some unit tests in place
- Add comments and clean up some code

##Getting Started
1. Review the sample in the "samples folder.
2. Add the `Microsoft.AspNetCore.Authentication.ActiveDirectory` library to your project.json file
3. Add the following line `services.AddAuthentication(options => new ActiveDirectoryCookieOptions());` in your Startup.cs `ConfigureServices` method
4. Add `app.UseCookieAuthentication(new ActiveDirectoryCookieOptions().ApplicationCookie);` to your Startup.cs `Configure` method
5. Add the following after the `app.UseCookieAuthentication()` line (Step 4):

```cs            
app.UseNtlm(new ActiveDirectoryOptions
{
    AutomaticAuthenticate = false,
    AutomaticChallenge = false,
    AuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
    SignInAsAuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
});
```
At this stage your middleware is accessible from the {site}/windowsauthentication/ntlm endpoint. ReturnUrl will take the user to the page after login.

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

##Remarks
Achieving server-side NTLM handshaking depends the Windows platform due to interop, dependency on domain-joined machines and secur32.dll. This will naturally limit this library to Windows-based DNX hosts.

See https://tools.ietf.org/html/rfc4559 for more info on NTLM

## Kudos
Most of the code here is based on what Yannic Staudt developed here: https://github.com/pysco68/Pysco68.Owin.Authentication.Ntlm
It is adapted for ASP.NET vNext with some changes to the logic. A HUGE thanks for the interop class!

##Contribution
Feel free to reach out, I would love to hear if you are using this (or trying to). Pull requests are more than welcome.
