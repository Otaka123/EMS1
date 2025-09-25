using Identity.Application.Contracts.Interfaces;
using Identity.Application.Contracts.Mapping;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services.PolicyProvider;
using Identity.Infrastructure.Services.Roles;
using Identity.Infrastructure.Services.Token;
using Identity.Infrastructure.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(
      this IServiceCollection services,
      string connectionString,
      Action<IdentityOptions>? configureIdentity = null)
        {
            // ✅ تسجيل DbContext أولاً
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ✅ إضافة Data Protection
            services.AddDataProtection()
                .SetApplicationName("MonstorApp");

            // ✅ إعدادات Identity الصحيحة
            services.AddIdentity<AppUser, ApplicationRole>(options =>
            {
                // إعدادات كلمة المرور
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // إعدادات المستخدم
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                // إعدادات القفل
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // إعدادات تسجيل الدخول
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddDefaultTokenProviders();

            // ✅ إذا كنت تريد تخصيص إعدادات الـ Cookies، استخدم ConfigureApplicationCookie
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Monstor.Identity";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

                options.LoginPath = "/Admin/User/Login";
                options.AccessDeniedPath = "/Admin/User/AccessDenied";
                options.LogoutPath = "/Admin/User/Logout";

                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = true;

                options.ReturnUrlParameter = "returnUrl";
            });

            // ✅ باقي الخدمات
            services.AddHttpClient();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddAutoMapper(typeof(UserMappingProfile));
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
        //public static IServiceCollection AddIdentityServices(
        //        this IServiceCollection services,
        //        string connectionString,
        //        Action<IdentityOptions>? configureIdentity = null)
        //{
        //    // تسجيل DbContext
        //    services.AddDbContext<AppIdentityDbContext>(options =>
        //   options.UseSqlServer(connectionString));

        //    // Identity يجب أن يستخدم AppIdentityDbContext
        //    services.AddIdentity<AppUser, ApplicationRole>(options =>
        //    {
        //        options.Password.RequireDigit = true;
        //        options.Password.RequiredLength = 8;
        //        options.User.RequireUniqueEmail = true;
        //    })
        //    .AddEntityFrameworkStores<AppIdentityDbContext>()  // ✅ نفس الـ DbContext
        //    .AddSignInManager<SignInManager<AppUser>>()
        //    .AddRoleManager<RoleManager<ApplicationRole>>()
        //    .AddDefaultTokenProviders();

        //    if (configureIdentity != null)
        //    {
        //        services.Configure(configureIdentity);
        //    }
        //    services.AddHttpClient(); // هذه السطر سيحل المشكلة

        //    // خدمات التطبيق
        //    services.AddScoped<IRoleService, RoleService>();
        //    //services.AddScoped<ISendEmailService, SendEmailService>();
        //    services.AddScoped<IJwtTokenService, JwtTokenService>();
        //    //services.AddScoped<IAccountService, AccountService>();
        //    //services.AddScoped<INotificationService, NotificationService>();
        //    services.AddScoped<IUserService, UserService>();
        //    //services.AddScoped<IRoleCacheService, RoleCacheService>();
        //    //services.AddScoped<IRoleService, RoleService>();
        //    services.AddAutoMapper(typeof(UserMappingProfile));
        //    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        //    //services.AddScoped<IPermissionHistoryService, PermissionHistoryService>();


        //    ////services.Configure<BrevoSettings>(services.Configuration.GetSection("Brevo"));
        //    //services.AddScoped<ISendEmailService, SendEmailService>();
        //    //services.AddScoped<INotificationService, NotificationService>();
        //    //// FluentValidation
        //    //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //    //services.AddScoped<IValidationService, ValidationService>();


        //    //// AutoMapper
        //    ////services.AddAutoMapper(typeof(AccountService).Assembly);
        //    //services.AddAutoMapper(typeof(UserMappingProfile));

        //    return services;
        //}
    }
}

