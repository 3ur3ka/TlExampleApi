using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TlApiExample.Helpers;
using TlApiExample.Services;

namespace TlApiExample
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

            if (Environment.GetEnvironmentVariable("USE_TRUELAYER_SANDBOX") == "false")
            {
                services.Configure<TrueLayerCredentials>(x => Configuration.GetSection("TrueLayerLiveCredentials").Bind(x));
            }
            else
            {
                services.Configure<TrueLayerCredentials>(x => Configuration.GetSection("TrueLayerSandboxCredentials").Bind(x));
            }

            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddControllers();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cookieOptions => cookieOptions.Cookie.Name = ".TlApiExample");

            // configure DI
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IHttpRequestService, HttpRequestService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
