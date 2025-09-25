using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Identity.Application.Exceptions;
using Identity.Infrastructure.Persistence;

namespace Identity.Infrastructure.Services.Token
{
 
 
        public class JwtTokenService : IJwtTokenService
        {
            private readonly IConfiguration _configuration;
            private readonly ILogger<JwtTokenService> _logger;
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly SymmetricSecurityKey _securityKey;
            private const string FacebookTokenValidationUrl = "https://graph.facebook.com/me?fields=id,name,email&access_token={0}";
            private readonly AppIdentityDbContext _dbContext;
            private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        public JwtTokenService(
                UserManager<AppUser> userManager,
                IConfiguration configuration,
                ILogger<JwtTokenService> logger,
                IHttpClientFactory httpClientFactory, AppIdentityDbContext dbContext,
                RoleManager<ApplicationRole> roleManager)
            {
                _userManager = userManager;
                _configuration = configuration;
                _logger = logger;
                _httpClientFactory = httpClientFactory;
                _dbContext = dbContext;
            _roleManager = roleManager;


                var secretKey = configuration["Jwt:SecretKey"]
                    ?? throw new ArgumentNullException("JWT Secret Key is missing.");
                _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            }

            public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken)
            {
                try
                {
                    var clientId = _configuration["Authentication:Google:ClientId"]
                        ?? throw new ArgumentNullException("Google Client ID is missing.");

                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    };

                    return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Google token validation failed.");
                    return null;
                }
            }
            public async Task<bool> RevokeRefreshTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        _logger.LogWarning("RevokeRefreshTokenAsync called with empty userId.");
                        return false;
                    }

                    // البحث عن جميع الـ RefreshTokens الخاصة بالمستخدم
                    var userTokens = await _dbContext.RefreshTokens
                        .Where(rt => rt.UserId == userId)
                        .ToListAsync(cancellationToken);

                    if (!userTokens.Any())
                    {
                        _logger.LogInformation("No refresh tokens found for user: {UserId}", userId);
                        return false;
                    }

                    // حذف التوكنات من قاعدة البيانات
                    _dbContext.RefreshTokens.RemoveRange(userTokens);
                    var affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Revoked {Count} refresh token(s) for user: {UserId}",
                        affectedRows, userId
                    );

                    return affectedRows > 0;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("RevokeRefreshTokenAsync operation cancelled for user: {UserId}", userId);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error revoking refresh tokens for user: {UserId}", userId);
                    return false;
                }
            }

            public async Task<string> VerifyFacebookToken(string accessToken)
            {
                var client = _httpClientFactory.CreateClient();
                var url = string.Format(FacebookTokenValidationUrl, accessToken);

                try
                {
                    var response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode
                        ? await response.Content.ReadAsStringAsync()
                        : null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Facebook token validation failed.");
                    return null;
                }
            }
            public async Task<string> GenerateJwtToken(
        AppUser user,
        List<string>? roles = null,
        CancellationToken cancellationToken = default)
            {
                // التحقق من المدخلات
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                }

                cancellationToken.ThrowIfCancellationRequested();

                // قراءة التكوينات
                var issuer = _configuration["Jwt:Issuer"]
                    ?? throw new ArgumentNullException("Jwt:Issuer is missing in configuration");

                var audience = _configuration["Jwt:Audience"]
                    ?? throw new ArgumentNullException("Jwt:Audience is missing in configuration");

                var expirationMinutes = _configuration.GetValue("Jwt:ExpirationMinutes", 30);

                // إنشاء الـ Claims
                var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
    };

                // إضافة الأدوار (بدون تكرار)
                if (roles?.Any() == true)
                {
                    claims.AddRange(
                        roles.Where(role => !string.IsNullOrWhiteSpace(role))
                             .Distinct()
                             .Select(role => new Claim(ClaimTypes.Role, role.Trim()))
                    );
                }

                cancellationToken.ThrowIfCancellationRequested();

                // إنشاء التوكن
                var signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    SigningCredentials = signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateLifetime = false, // 👈 نتجاهل انتهاء الصلاحية هنا
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }


        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        //لو حصل مشكله هنا بسبب انك شيلت nullable
        public async Task<LoginDTO> GenerateJwtWithRefreshTokenAsync(
    AppUser user,
    List<string> roles ,
    CancellationToken cancellationToken = default)
            {
                // توليد JWT
                var accessToken = await GenerateJwtToken(user, roles, cancellationToken);

                // توليد RefreshToken
                var refreshToken = GenerateRefreshToken();

                var tokenEntry = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7) // صلاحية أسبوع مثلاً
                };

                // تحديث المستخدم في قاعدة البيانات
                _dbContext.RefreshTokens.Add(tokenEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);
                LoginDTO loginDTO = new LoginDTO(accessToken, refreshToken);
                return loginDTO;
            }
        public async Task<LoginDTO> GenerateJwtWithRefreshTokenAsync(
 AppUser user,
 CancellationToken cancellationToken = default)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var userPermissions = new List<Permission>();
            if (roles.Any())
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null) continue;

                    var rolePermissions = await _dbContext.RolePermissions
                        .Where(rp => rp.RoleId == role.Id)
                        .Select(rp => rp.Permission)
                        .ToListAsync(cancellationToken);

                    userPermissions.AddRange(rolePermissions);
                }
            }
            // توليد JWT يشمل Roles و Permissions
            var accessToken = await GenerateJwtToken(user, roles, userPermissions, cancellationToken);

            // توليد RefreshToken
            var refreshToken = GenerateRefreshToken();

            var tokenEntry = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7) // صلاحية أسبوع
            };

            // حفظ RefreshToken في قاعدة البيانات
            _dbContext.RefreshTokens.Add(tokenEntry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // إرجاع DTO يحتوي على AccessToken و RefreshToken
            return new LoginDTO(accessToken, refreshToken);
        }
        public async Task<string> GenerateJwtToken(
    AppUser user,
    List<string>? roles = null,
    List<Permission>? userPermissions = null,
    CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            cancellationToken.ThrowIfCancellationRequested();

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new ArgumentNullException("Jwt:Issuer is missing in configuration");
            var audience = _configuration["Jwt:Audience"]
                ?? throw new ArgumentNullException("Jwt:Audience is missing in configuration");
            var expirationMinutes = _configuration.GetValue("Jwt:ExpirationMinutes", 30);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
                  DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                  ClaimValueTypes.Integer64)
    };

            // إضافة الأدوار
            if (roles?.Any() == true)
            {
                claims.AddRange(
                    roles.Where(r => !string.IsNullOrWhiteSpace(r))
                         .Distinct()
                         .Select(r => new Claim(ClaimTypes.Role, r.Trim()))
                );
            }

            // إضافة الصلاحيات
            if (userPermissions?.Any() == true)
            {
                claims.AddRange(
                    userPermissions.Select(p =>
                        new Claim("Permission", $"{p.Category}:{p.PermissionType}:{p.Name}"))
                );
            }

            var signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateJwtToken(AppUser user, List<string>? roles = null)
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User cannot be null");

                var issuer = _configuration["Jwt:Issuer"]
                    ?? throw new ArgumentNullException("Jwt:Issuer is missing in configuration");

                var audience = _configuration["Jwt:Audience"]
                    ?? throw new ArgumentNullException("Jwt:Audience is missing in configuration");

                var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
                  DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                  ClaimValueTypes.Integer64)
    };

                // إضافة الدور
                if (roles?.Any() == true)
                {
                    foreach (var role in roles)
                    {
                        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Trim())));
                    }
                }

                var signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpirationMinutes", 30)),
                    SigningCredentials = signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }



            //public async Task<string> GenerateJwtToken(AppUser user, List<string>? roles = null)
            //{
            //    if (user == null)
            //    {
            //        _logger.LogError("User is null.");
            //        return null;
            //    }

            //    //if (roles == null || !roles.Any())
            //    //{
            //    //    _logger.LogError("No roles provided for user {UserId}.", user.Id);
            //    //    return null;
            //    //}

            //    var issuer = _configuration["Jwt:Issuer"]
            //        ?? throw new ArgumentNullException("JWT Issuer is missing.");
            //    var audience = _configuration["Jwt:Audience"]
            //        ?? throw new ArgumentNullException("JWT Audience is missing.");

            //    var claims = new List<Claim>
            //{
            //    new(JwtRegisteredClaimNames.Sub, user.Id),
            //    //new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //    //new(ClaimTypes.Email, user.Email)
            //};
            //    if (roles == null || !roles.Any())
            //    {
            //        _logger.LogWarning("No roles provided for user {UserId}. Token will contain no role claims.", user.Id);
            //    }
            //    else
            //    {
            //        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Trim())));
            //    }

            //    //claims.AddRange((roles ?? new List<string>()).Select(role => new Claim(ClaimTypes.Role, role.Trim())));

            //    var creds = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
            //    int expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes", 30);
            //    var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            //    var token = new JwtSecurityToken(
            //        issuer: issuer,
            //        audience: audience,
            //        claims: claims,
            //        expires: expiration,
            //        signingCredentials: creds
            //    );

            //    _logger.LogInformation("Generated JWT token for {Email} (Expires: {Expiration})",
            //        user.Email, expiration);

            //    return new JwtSecurityTokenHandler().WriteToken(token);
            //}
        }
    
}
