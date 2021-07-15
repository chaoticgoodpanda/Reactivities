using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Application.Activities;
using Application.Core;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Persistence;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(opt =>
            {
                //adding authorization policy. Ensures every single endpoint in API requires authentication unless we tell it otherwise.
                //Don't need [Authorize] attribute in controllers anymore.
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddFluentValidation(config =>
            {
                config.RegisterValidatorsFromAssemblyContaining<Create>();
            });

            //housekeeping, keeps startup classes tidy by calling method of services list in Extensions file.
            services.AddApplicationServices(_config);
            services.AddIdentityServices(_config);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //custom exception handling middleware
            app.UseMiddleware<ExceptionMiddleware>();
            
            //adding security
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opt => opt.NoReferrer());
            app.UseXXssProtection(opt => opt.EnabledWithBlockMode());
            app.UseXfo(opt => opt.Deny());
            //limit to it only being OK for us to run JavaScript from our own domain
            //report only
            app.UseCsp(opt => opt
                .BlockAllMixedContent()
                .StyleSources(s => s.Self()
                    .CustomSources("https://fonts.googleapis.com",
                        "sha256-oFySg82XYSNiSd+Q3yfYPD/rxY6RMDMJ0KxzGG74iGM="))
                .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com", "data:"))
                .FormActions(s => s.Self())
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self()
                    .CustomSources("https://res.cloudinary.com", 
                        "blob:",
                        "data:",
                        "https://www.facebook.com",
                        "https://platform.lookaside.fbsbx.com",
                        "https://platform-lookaside.fbsbx.com"))
                .ScriptSources(s => s.Self()
                    .CustomSources("sha256-1Xd5VbAO++yNbo24/AtILK1tKVB0ixBbThpwpSoR8lk=",
                        "https://connect.facebook.net",
                        "sha256-r+c+WXnu5AXAx7eduEl7TO8MtOpQXGaeFznNYwGK7wc="))
                .ObjectSources(s => s.None())
            );
            
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }
            else
            {
                //implementing Strict Transport Security
                //this normally works, but has issues with Heroku
                //app.UseHsts();
                app.Use(async (context, next) =>
                {
                    //valid for one year (in seconds)
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
                    await next.Invoke();
                });
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            
            //for npm build - for production
            //looks for anything in our root folder that has www.html
            app.UseDefaultFiles();
            //by default serves static files from wwwroot folder
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
            //JWT Token authentication has to go right before Authorization.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //adding SignalR endpoint for real-time chat
                endpoints.MapHub<ChatHub>("/chat");
                //create controller with an action called "Index" named "Fallback"
                endpoints.MapFallbackToController("Index", "Fallback");
            });

            
        }
    }
}
