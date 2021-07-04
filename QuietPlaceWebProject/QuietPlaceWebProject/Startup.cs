using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject
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
            string[] paths =
            {
                Environment.CurrentDirectory + @"\App_Data\BoardDB.mdf",
                Environment.CurrentDirectory + @"\App_Data\UserDB.mdf"
            };
            
            string[] connectionStrings =
            {
                "Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\'" + paths[0] + "\';Trusted_Connection=True;MultipleActiveResultSets=true;",
                "Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\'" + paths[1] + "\';Trusted_Connection=True;MultipleActiveResultSets=true;"
            };

            if (!File.Exists(paths[0]))
                connectionStrings[0] = Configuration.GetConnectionString("BoardConnection");
            
            if (!File.Exists(paths[1]))
                connectionStrings[1] = Configuration.GetConnectionString("UserConnection");

            services.AddDbContext<BoardContext>(options => options.UseSqlServer(connectionStrings[0]));
            services.AddDbContext<UserContext>(options => options.UseSqlServer(connectionStrings[1]));
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Anon/NotFoundPage");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Anon/NotFoundPage");
                });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Board/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();    // аутентификация
            app.UseAuthorization();     // авторизация

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Board}/{action=Boards}/{id?}");
            });
        }
    }
}