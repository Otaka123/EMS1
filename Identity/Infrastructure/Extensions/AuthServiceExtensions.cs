using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
//"JwtBearer",
namespace Identity.Infrastructure.Extensions
{
    public static class AuthServiceExtensions
    {

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
.AddJwtBearer( options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience =false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        //ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]
                ?? throw new ArgumentNullException("JWT key missing"))
        ),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // ✅ لو حابب تقرأ التوكن من الكوكي (مثلاً من MVC)
            var token = context.Request.Cookies["JWT_TOKEN"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

            return services;
        }
        //        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration config)
        //    {
        //        services.AddAuthentication(options =>
        //        {
        //            // JWT للواجهات API، Cookies للواجهات الأمامية
        //            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        })
        //        .AddCookie(options =>
        //        {
        //            options.LoginPath = "/Admin/User/Login"; // عدل هذا المسار حسب تطبيقك
        //            options.AccessDeniedPath = "/Admin/User/AccessDenied";
        //            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);

        //            options.SlidingExpiration = true;
        //            options.Cookie.HttpOnly = true;
        //            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        //            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        //        })
        //        .AddJwtBearer(options =>
        //        {
        //            options.TokenValidationParameters = new TokenValidationParameters
        //            {
        //                ValidateIssuer = true,
        //                ValidateAudience = true,
        //                ValidateLifetime = true,
        //                ValidateIssuerSigningKey = true,
        //                ValidIssuer = config["Jwt:Issuer"],
        //                ValidAudience = config["Jwt:Audience"],
        //                IssuerSigningKey = new SymmetricSecurityKey(
        //                    Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]
        //                        ?? throw new ArgumentNullException("JWT key missing"))
        //                ),
        //                RoleClaimType = ClaimTypes.Role,
        //                ClockSkew = TimeSpan.Zero
        //            };

        //            // ✅ قراءة التوكن من الكوكي
        //            options.Events = new JwtBearerEvents
        //            {
        //                OnMessageReceived = context =>
        //                {
        //                    var token = context.Request.Cookies["JWT_TOKEN"];
        //                    if (!string.IsNullOrEmpty(token))
        //                    {
        //                        context.Token = token;
        //                    }
        //                    return Task.CompletedTask;
        //                }
        //            };
        //        });

        //        return services;
        //    }
        //}
        //services.AddAuthentication(options =>
        //{
        //    // نخلي الافتراضي JwtBearer
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //})
        //.AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidIssuer = config["Jwt:Issuer"],
        //        ValidAudience = config["Jwt:Audience"],
        //        IssuerSigningKey = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]
        //                ?? throw new ArgumentNullException("JWT key missing"))
        //        ),
        //        RoleClaimType = ClaimTypes.Role,
        //        ClockSkew = TimeSpan.Zero
        //    };

        //    // نخلي JwtBearer يقرأ التوكن من الكوكي
        //    options.Events = new JwtBearerEvents
        //    {
        //        OnMessageReceived = context =>
        //        {
        //            var token = context.Request.Cookies["JWT_TOKEN"];
        //            if (!string.IsNullOrEmpty(token))
        //            {
        //                context.Token = token;
        //            }
        //            return Task.CompletedTask;
        //        }
        //    };
        //});

        //    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration config)
        //    {
        //        //services.AddAuthentication(options =>
        //        //{
        //        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //        //})
        //        //.AddJwtBearer(options =>
        //        //{
        //        //    options.TokenValidationParameters = new TokenValidationParameters
        //        //    {
        //        //        ValidateIssuer = true,
        //        //        ValidateAudience = true,
        //        //        ValidateLifetime = true,
        //        //        ValidateIssuerSigningKey = true,
        //        //        ValidIssuer = config["Jwt:Issuer"],
        //        //        ValidAudience = config["Jwt:Audience"],
        //        //        IssuerSigningKey = new SymmetricSecurityKey(
        //        //            Encoding.UTF8.GetBytes(config["Jwt:SecretKey"] ?? throw new ArgumentNullException("JWT key missing"))
        //        //        ),
        //        //        RoleClaimType = ClaimTypes.Role,
        //        //        ClockSkew = TimeSpan.Zero

        //        //    };


        //        services.AddAuthentication(options =>
        //        {
        //            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //        })
        //.AddCookie(options =>
        //{
        //    options.LoginPath = "/Admin/User/Login";
        //    options.LogoutPath = "/Admin/User/SignOut";
        //    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        //    options.SlidingExpiration = true;
        //})
        //.AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        ValidateIssuerSigningKey = true,
        //        ValidIssuer = config["Jwt:Issuer"],
        //        ValidAudience = config["Jwt:Audience"],
        //        IssuerSigningKey = new SymmetricSecurityKey(
        //            Encoding.UTF8.GetBytes(config["Jwt:SecretKey"] ?? throw new ArgumentNullException("JWT key missing"))
        //        ),
        //        RoleClaimType = ClaimTypes.Role,
        //        ClockSkew = TimeSpan.Zero
        //    };
        //});



        //        return services;
        //    }
        //}
    }
}