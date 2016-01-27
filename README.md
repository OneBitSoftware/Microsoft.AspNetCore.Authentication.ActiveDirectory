# Microsoft.AspNetCore.Authentication.ActiveDirectory
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

##Remarks
Achieving server-side NTLM handshaking depends the Windows platform due to interop, dependency on domain-joined machines and secur32.dll. This will naturally limit this library to Windows-based DNX hosts.

See https://tools.ietf.org/html/rfc4559 for more info on NTLM

## Kudos
Most of the code here is based on what Yannic Staudt developed here: https://github.com/pysco68/Pysco68.Owin.Authentication.Ntlm
It is adapted for ASP.NET vNext with some changes to the logic. A HUGE thanks for the interop class!

##Contribution
Feel free to submit a pull request or contact me for using this.
