using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.ActiveDirectory;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Authentication;

namespace Sample_AspNet5.Mvc6.Ntlm
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => new ActiveDirectoryCookieOptions());

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            var cookieOptions = new ActiveDirectoryCookieOptions();
            //cookieOptions.AuthenticationScheme = ActiveDirectoryCookieOptions
            //cookieOptions.LoginPath = new PathString("/windowsauthentication/ntlm");
            //cookieOptions.AutomaticChallenge = false;
            //cookieOptions.AutomaticAuthenticate = false;

            //Provider = new CookieAuthenticationProvider()
            //{
            //    OnApplyRedirect = ctx =>
            //    {
            //        if (!ctx.Request.IsNtlmAuthenticationCallback())    // <------
            //        {
            //            ctx.Response.Redirect(ctx.RedirectUri);
            //        }
            //    }
            //}

            app.UseCookieAuthentication(cookieOptions.ApplicationCookie);

            app.UseNtlm(new ActiveDirectoryOptions
            {
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,
                //CallbackPath = new PathString("/windowsauthentication/ntlm"),
                AuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
                SignInAsAuthenticationScheme = ActiveDirectoryOptions.DefaultAuthenticationScheme,
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
