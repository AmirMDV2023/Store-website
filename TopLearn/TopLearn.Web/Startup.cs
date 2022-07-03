using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopLearn.Core.Convertors;
using TopLearn.Core.Services;
using TopLearn.Core.Services.Interfaces;
using TopLearn.DataLayer.Context;


namespace TopLearn.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false);
            //services.AddControllersWithViews();
            //services.AddMvc();
            services.AddRazorPages();
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = int.MaxValue;
            });
            #region Authentication

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme= CookieAuthenticationDefaults.AuthenticationScheme;

            }).AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
                options.ExpireTimeSpan=TimeSpan.FromMinutes(43200);

            });

            #endregion

            #region DataBase Context

            services.AddDbContext<TopLearnContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("TopLearnConnection"));
                }
            );

            #endregion

            #region IoC

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IViewRenderService, RenderViewToString>();
            services.AddTransient<IPermissionService, PermissionService>();
            services.AddTransient<ICourseService,CourseService>();
            services.AddTransient<IOrderService, OrderService>();

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value.ToString().ToLower().StartsWith("/coursefilesonline"))
                {
                    var host = context.Request.Host.Value.ToString();
                    var callingUrl = context.Request.Headers["Referer"].ToString();
                    if (callingUrl!=""&& (callingUrl.StartsWith("https://"+host) || callingUrl.StartsWith("https://"+host)))
                    {
                        await next.Invoke();
                    }
                    else
                    {
                        context.Response.Redirect("/Login");
                    }
                }
                else
                {
                    await next.Invoke();
                }

                
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMvc();
            app.UseDeveloperExceptionPage();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(

                    name: "MyAreas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
                endpoints.MapControllerRoute(
                
                    name:"Default",
                    pattern:"{controller=Home}/{action=Index}/{id?}"
                );
            });
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
