using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Middleware
{
    public class JwtRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var jwtToken = context.Request.Cookies["JWT_TOKEN"];
            var refreshToken = context.Request.Cookies["REFRESH_TOKEN"];

            if (!string.IsNullOrEmpty(jwtToken) && JwtExpired(jwtToken))
            {
                var result = await userService.RefreshTokenAsync(new RefreshtokenRequest
                {
                    refreshToken = refreshToken
                }, CancellationToken.None);

                if (result.IsSuccess)
                {
                    var jwtCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(15)
                    };

                    var refreshCookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(30)
                    };

                    context.Response.Cookies.Append("JWT_TOKEN", result.Data.AccessToken, jwtCookieOptions);
                    context.Response.Cookies.Append("REFRESH_TOKEN", result.Data.RefreshToken, refreshCookieOptions);
                }
            }

            await _next(context);
        }

        private bool JwtExpired(string token)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
    }

}
