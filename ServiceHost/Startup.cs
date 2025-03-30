﻿using AccountManagement.Configuration;
using AccountManagement.Domain.Account.Agg;
using AccountManagement.Infrastructure.EFCore;
using BlogManagement.Configuration;
using CommentManagement.Configuration;
using EventManagement.Configuration;
using GeneralManagement.Configuration;
using LanguageManagement.Application.Contracts.Language;
using LanguageManagement.Configuration;
using LanguageManagement.Infrastructure.EFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using My_Shop_Framework.Application;
using My_Shop_Framework.Application.ZarinPal;
using ServiceHost.Model;
using ServiceHost.Utils;
using ShopManagement.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ServiceHost
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

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 92428800;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 92428800;
            });
            services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();


            services.AddIdentity<ApplicationUser, ApplicationRole>()
               .AddRoleManager<RoleManager<ApplicationRole>>()
               .AddSignInManager<CustomSignInManager>()
               .AddEntityFrameworkStores<AccountContext>()
               .AddDefaultTokenProviders()
               .AddErrorDescriber<CustomIdentityErrorDescriber>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.User.RequireUniqueEmail = false;
       

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

            
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedPhoneNumber = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/AccountRegister/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(40);
                options.SlidingExpiration = true;

                // options.Events.OnRedirectToLogin = context =>
                // {
                //     context.RedirectUri = "/Identity/Account/AccessDenied";
                //     context.Response.StatusCode = 403;
                //     return Task.CompletedTask;
                // };
            });


            services.AddAuthorization(options =>
             {
                 options.AddPolicy("SuperAdmin", policy =>
                     policy.RequireRole("SuperAdmin"));

                 options.AddPolicy("UserPanel", policy =>
                     policy.RequireRole("SuperAdmin", "Admin", "DistributionUser"));

                 options.AddPolicy("Admin", policy =>
                     policy.RequireRole("SuperAdmin", "Admin"));

                 options.AddPolicy("UsersList", policy =>
                   policy.RequireRole("SuperAdmin", "Admin", "ManageUser"));

             });

            
            services.AddHttpContextAccessor();
            var connectionString = Configuration.GetConnectionString("HoldenHarvest_Db");
            ShopManagementBoostStrapper.Configure(services, connectionString);
            BlogManagementBootStrapper.Configure(services, connectionString);
            CommentManagementBoostStrapper.Configure(services, connectionString);
            AccountManagementBootStrapper.Configure(services, connectionString);
            LanguageManagementBootStrapper.Configure(services, connectionString);
            GeneralManagementBootStrapper.Configure(services, connectionString);
            EventManagementBootStrapper.Configure(services, connectionString);

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<EmailSender>();
            services.AddTransient<IFileUploader, FileUploader>();
      


            services.AddTransient<IZarinPalFactory, ZarinPalFactory>();
       

            services.AddScoped<LibraryInitializer>();
            services.AddScoped<LibraryInitializerLanguage>();
            

            services.AddLocalization(option => option.ResourcesPath = "Resources");
           




            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix,
                    option => { option.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factor) =>
                        factor.Create(typeof(ShareResource));
                });




            services.AddRazorPages();

          
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILanguageApplication languageApplication, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // app.UseExceptionHandler("/ErrorPage"); // خطاهای اجرا به این مسیر هدایت می‌شوند
                app.UseStatusCodePagesWithReExecute("/NotFound"); // خطاهای وضعیت (مانند 404)
                app.UseHsts(); // فعال‌سازی HSTS در محیط Production
            }



            app.UseHttpsRedirection();

            app.UseStaticFiles();


            app.UseCookiePolicy();

            /*using (var scope = app.ApplicationServices.CreateScope())
            {
                var initializer2 = scope.ServiceProvider.GetRequiredService<LibraryInitializerLanguage>();
                initializer2.Initialize();
            }*/

            var languageList = languageApplication.List();
            var supportCulture = new List<CultureInfo>();

            foreach (var language in languageList)
            {
                supportCulture.Add(new CultureInfo(language.LanguageTitle));
            }

            var options = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportCulture,
                SupportedUICultures = supportCulture,
                RequestCultureProviders = new List<IRequestCultureProvider>()
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                }
            };

            app.UseRequestLocalization(options);
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                // endpoints.MapControllers();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

           /* CreateUserRoles(services).Wait();*/

            using var serviceScope = app.ApplicationServices.CreateScope();
            var initializer = serviceScope.ServiceProvider.GetRequiredService<LibraryInitializer>();
            initializer.Initialize();
        }
        #region CreateRoleAndUser

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {

            var RoleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            #region AddRoles

            if (!await RoleManager.RoleExistsAsync("SuperAdmin"))
                await RoleManager.CreateAsync(RoleCreator("SuperAdmin", "مدیر سیستم"));

            if (!await RoleManager.RoleExistsAsync("Admin"))
                await RoleManager.CreateAsync(RoleCreator("Admin", "ادمین"));

            if (!await RoleManager.RoleExistsAsync("ManageUser"))
                await RoleManager.CreateAsync(RoleCreator("ManageUser", "لیست کاربران"));

            if (!await RoleManager.RoleExistsAsync("RegularUser"))
                await RoleManager.CreateAsync(RoleCreator("RegularUser", " کاربر عادی"));
            if (!await RoleManager.RoleExistsAsync("DistributionUser"))
                await RoleManager.CreateAsync(RoleCreator("DistributionUser", " کاربر توزیع"));
            #endregion

            var UnknownUserEmail = "sajjadgh1995@hotmail.com";
            var UnknownUserPassword = "123@asdfA";
            ApplicationUser UnknownUser = await UserManager.FindByNameAsync("0");

            if (UnknownUser == null)
            {
                await UserManager.CreateAsync(new ApplicationUser
                {
                    UserName = "0",
                    FirstName = "Unknown",
                    LastName = "User",
                    LockoutEnabled = false,
                }, UnknownUserPassword);

                UnknownUser = await UserManager.FindByEmailAsync(UnknownUserEmail);
            }
            var superAdminEmail = "sajjadgh1995@hotmail.com";
            var superAdminPass = "123@asdfA";
            ApplicationUser user = await UserManager.FindByNameAsync("1");

            if (user == null)
            {
                await UserManager.CreateAsync(new ApplicationUser
                {
                    Email = superAdminEmail,
                    UserName = "1",
                    FirstName = "Admin",
                    LastName = "System",
                    PhoneNumber = "09938011131",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed =true,
                }, superAdminPass);

                user = await UserManager.FindByEmailAsync(superAdminEmail);
            }

            await UserManager.AddToRoleAsync(user, "SuperAdmin");
        }

        private static ApplicationRole RoleCreator(string name, string persianName)
        {
            return new ApplicationRole()
            {
                Name = name,
                PersianRoleName = persianName
            };
        }

        #endregion
    }
}