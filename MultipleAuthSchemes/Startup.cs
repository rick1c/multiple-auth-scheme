using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MultipleAuthSchemes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "OAuthTest";
            })
            .AddCookie()
             .AddOAuth("OAuthTest", options =>
             {
                 options.ClientId = Configuration["HMRC:ClientId"];
                 options.ClientSecret = Configuration["HMRC:ClientSecret"];
                 options.CallbackPath = new PathString("/account/auth-redirect");

                 options.AuthorizationEndpoint = "https://test-api.service.hmrc.gov.uk/oauth/authorize";
                 options.TokenEndpoint = "https://test-api.service.hmrc.gov.uk/oauth/token";
                 options.Scope.Add("read:vat");

                 options.SaveTokens = true;

                 options.Events = new OAuthEvents
                 {
                     OnCreatingTicket = async context =>
                     {
                         var claims = new List<Claim>
                         {
                                                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                         };
                         var identity = new ClaimsIdentity(claims, "OAuthTest", ClaimTypes.NameIdentifier, null);

                         context.Principal.AddIdentity(identity);
                     }
                 };
             });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
