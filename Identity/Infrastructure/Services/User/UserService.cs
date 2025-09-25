using AutoMapper;
using Common;
using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Common.Application.Contracts.Interfaces;
using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Common;
using Identity.Application.Contracts.DTO.Request.Users;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Enum;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Identity.Infrastructure.Services.User.UserService;

namespace Identity.Infrastructure.Services.User
{
    public class UserService : IUserService
    {

            private IMapper _mapper;
            private readonly ISharedMessageLocalizer _localizer;
            //private readonly INotificationService _notificationService;
            private readonly UserManager<AppUser> _userManager;
            private readonly SignInManager<AppUser> _signInManager;
            //private readonly IIntegrationEventsSender _eventsSender;
            private readonly AppIdentityDbContext _dbContext;
            //private readonly IValidator<LoginRequest> _loginvalidator;
            //private readonly IValidator<RegisterRequest> _registervalidator;
            private readonly IValidationService _validationService;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly ILogger<UserService> _logger;
            private readonly IServer _server;
            //private readonly IEmailService _emailService;
            private readonly IJwtTokenService _tokenService;
            private readonly ICurrentUserService _currentUserService;
            private const string DefaultRole = "User"; // الدور الافتراضي للمستخدمين الجدد
                                                           //IIntegrationEventsSender eventsSender,
                                                       //, IEmailService emailService
            public UserService(ISharedMessageLocalizer sharedLocalizer, UserManager<AppUser> userManager, ILogger<UserService> logger, IValidationService validationService,
                SignInManager<AppUser> signInManager, IHttpContextAccessor httpContextAccessor, RoleManager<ApplicationRole> roleManager,
                AppIdentityDbContext dbContext, IServer server, IJwtTokenService tokenService, IMapper mapper,ICurrentUserService currentUser)
            {
                _currentUserService = currentUser;
                _localizer = sharedLocalizer;
                _roleManager = roleManager;
                _logger = logger;
                _validationService = validationService;
                _userManager = userManager;
                _signInManager = signInManager;
                _httpContextAccessor = httpContextAccessor;
                //_eventsSender = eventsSender;
                _dbContext = dbContext;
                _server = server;
                _tokenService = tokenService;
            //_emailService = emailService;
                _mapper = mapper;
        }

        public async Task<RequestResponse<LoginDTO>> UserPasswordSignInAsync(
            Common.Contracts.DTO.Request.User.LoginRequest request,
            CancellationToken cancellationToken = default)
            {

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // التحقق من صحة المدخلات
                    var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return RequestResponse<LoginDTO>.BadRequest(
                            errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                        );
                    }

                    // العثور على المستخدم بأفضل أداء
                    var user = await FindUserAsync(request.UserNameOrEmailOrPhone, cancellationToken);
                    if (user == null || !user.IsActive)
                    {
                        _logger.LogWarning("Login failed for identifier: {Identifier} - User not found or inactive",
                            MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                        return RequestResponse<LoginDTO>.Unauthorized(_localizer["InvalidCredentials"]);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // التحقق من كلمة المرور
                    var result = await _signInManager.CheckPasswordSignInAsync(
                        user,
                        request.Password,
                        lockoutOnFailure: true
                    );

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Account locked out for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Locked(_localizer["AccountLocked"]);
                    }

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Unauthorized(_localizer["InvalidCredentials"]);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // توليد التوكن
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = await _tokenService.GenerateJwtWithRefreshTokenAsync(user, roles.ToList(), cancellationToken);
                    bool isupdated = await SetLastLoginDate(user); // تحديث تاريخ آخر تسجيل دخول
                    if (!isupdated)
                    {
                        _logger.LogWarning("Failed to update last login date for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Fail("\"Failed to update last login date"); // 499 Client Closed Request

                    }
                    _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                    return RequestResponse<LoginDTO>.Ok(
                        data: new LoginDTO(token.AccessToken, token.RefreshToken),
                        message: _localizer["LoginSuccessful"]
                    );
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Login operation was cancelled for identifier: {Identifier}",
                        MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                    return RequestResponse<LoginDTO>.Fail("Operation was cancelled"); // 499 Client Closed Request
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login for identifier: {Identifier}",
                        MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                    return RequestResponse<LoginDTO>.InternalServerError(_localizer["SystemError"]);
                }
            }
            public async Task<RequestResponse<LoginDTO>> SignInAsync(
           Common.Contracts.DTO.Request.User.LoginRequest request,
           CancellationToken cancellationToken = default)
            {

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // التحقق من صحة المدخلات
                    var validationResult = await _validationService.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        return RequestResponse<LoginDTO>.BadRequest(
                            errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                        );
                    }

                    // العثور على المستخدم بأفضل أداء
                    var user = await FindUserAsync(request.UserNameOrEmailOrPhone, cancellationToken);
                    if (user == null || !user.IsActive)
                    {
                        _logger.LogWarning("Login failed for identifier: {Identifier} - User not found or inactive",
                            MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                        return RequestResponse<LoginDTO>.Unauthorized(_localizer["InvalidCredentials"]);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // التحقق من كلمة المرور
                    var result = await _signInManager.CheckPasswordSignInAsync(
                        user,
                        request.Password,
                        lockoutOnFailure: true
                    );

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Account locked out for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Locked(_localizer["AccountLocked"]);
                    }

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Unauthorized(_localizer["InvalidCredentials"]);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // توليد التوكن
                    var token = await _tokenService.GenerateJwtWithRefreshTokenAsync(user, cancellationToken);
                    bool isupdated = await SetLastLoginDate(user); // تحديث تاريخ آخر تسجيل دخول
                    if (!isupdated)
                    {
                        _logger.LogWarning("Failed to update last login date for user: {UserId}", user.Id);
                        return RequestResponse<LoginDTO>.Fail("\"Failed to update last login date"); // 499 Client Closed Request

                    }
                    _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                    return RequestResponse<LoginDTO>.Ok(
                        data: new LoginDTO(token.AccessToken, token.RefreshToken),
                        message: _localizer["LoginSuccessful"]
                    );
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Login operation was cancelled for identifier: {Identifier}",
                        MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                    return RequestResponse<LoginDTO>.Fail("Operation was cancelled"); // 499 Client Closed Request
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login for identifier: {Identifier}",
                        MaskSensitiveExtensions(request.UserNameOrEmailOrPhone));
                    return RequestResponse<LoginDTO>.InternalServerError(_localizer["SystemError"]);
                }
            }

            public async Task<RequestResponse> SignOutAsync(CancellationToken cancellationToken = default)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // الحصول على المستخدم الحالي من الـ context
                    if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == null)
                    {
                        _logger.LogWarning("Sign-out failed: no authenticated user found.");
                        return RequestResponse.Unauthorized(_localizer["UserNotAuthenticated"]);
                    }
                    var userid = _currentUserService.UserId;


                    cancellationToken.ThrowIfCancellationRequested();

                    // إبطال أو حذف الـ Refresh Token من قاعدة البيانات
                    var refreshTokenRemoved = await _tokenService.RevokeRefreshTokenAsync(userid, cancellationToken);
                    //if (!refreshTokenRemoved)
                    //{
                    //    _logger.LogWarning("Failed to revoke refresh token for user: {UserId}", userid);
                    //    return RequestResponse.Fail("Failed to revoke refresh token");
                    //}

                    cancellationToken.ThrowIfCancellationRequested();

                    // تسجيل الخروج من ASP.NET Identity
                    await _signInManager.SignOutAsync();

                    _logger.LogInformation("User {UserId} signed out successfully", userid);
                    return RequestResponse.Ok(message: _localizer["SignOutSuccessful"]);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Sign-out operation was cancelled.");
                    return RequestResponse.Fail("Operation was cancelled");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during sign-out.");
                    return RequestResponse.InternalServerError(_localizer["SystemError"]);
                }
            }

            public async Task<RequestResponse<LoginDTO>> RefreshTokenAsync(
      RefreshtokenRequest loginDTO,
          CancellationToken cancellationToken = default)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // استخراج الـ UserId من الـ AccessToken (حتى لو منتهي)
                    var principal = _tokenService.GetPrincipalFromExpiredToken(loginDTO.accessToken);
                    if (principal == null)
                    {
                        return RequestResponse<LoginDTO>.Unauthorized("Invalid access token.");
                    }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    {
                        return RequestResponse<LoginDTO>.Unauthorized("User identifier not found in token.");
                    }

                    // التحقق من وجود RefreshToken في قاعدة البيانات ومطابقته للمستخدم
                    var tokenEntry = await _dbContext.RefreshTokens
                        .FirstOrDefaultAsync(
                            x => x.Token == loginDTO.refreshToken
                              && x.UserId == userId
                              && !x.IsRevoked
                              && x.ExpiryDate > DateTime.UtcNow,
                            cancellationToken);

                    if (tokenEntry == null)
                    {
                        return RequestResponse<LoginDTO>.Unauthorized("Invalid or expired refresh token.");
                    }

                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null || !user.IsActive)
                    {
                        return RequestResponse<LoginDTO>.Unauthorized("Invalid user.");
                    }

                    var roles = await _userManager.GetRolesAsync(user);
                    var newTokens = await _tokenService.GenerateJwtWithRefreshTokenAsync(user, roles.ToList(), cancellationToken);

                    // إبطال التوكن القديم
                    tokenEntry.IsRevoked = true;
                    _dbContext.RefreshTokens.Update(tokenEntry);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    return RequestResponse<LoginDTO>.Ok(
                        new LoginDTO(newTokens.AccessToken, newTokens.RefreshToken),
                        "Token refreshed successfully"
                    );
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogWarning(ex, "Invalid access token during refresh.");
                    return RequestResponse<LoginDTO>.Unauthorized("Invalid access token.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while refreshing token.");
                    return RequestResponse<LoginDTO>.InternalServerError("System error.");
                }
            }
        public async Task<RequestResponse<List<UserInRoleDTO>>> GetUsersInRoleAsync(
    string roleId,
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return RequestResponse<List<UserInRoleDTO>>.NotFound(_localizer["Roles.RoleNotFound"]);
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name);

                var userDtos = users.Where(u => u.IsActive).Select(u => new UserInRoleDTO
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FirstName + " " + u.LastName,
                    IsActive = u.IsActive
                }).ToList();

                return RequestResponse<List<UserInRoleDTO>>.Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users in role {RoleId}", roleId);
                return RequestResponse<List<UserInRoleDTO>>.InternalServerError(_localizer["SystemError"]);
            }
        }
        public async Task<RequestResponse<List<UserInRoleDTO>>> GetAllUsersAsync(
    CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var users = await _userManager.Users
                    .Where(u => u.IsActive)
                    .Select(u => new UserInRoleDTO
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        FullName = u.FirstName+" "+u.LastName,
                        IsActive = u.IsActive
                    })
                    .ToListAsync(cancellationToken);

                return RequestResponse<List<UserInRoleDTO>>.Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return RequestResponse<List<UserInRoleDTO>>.InternalServerError(_localizer["SystemError"]);
            }
        }
        /// <summary>
        /// البحث عن مستخدم باستخدام المعرف (بريد إلكتروني، رقم هاتف، أو اسم مستخدم)
        /// </summary>
        /// <param name="identifier">المعرف (Email/Phone/Username)</param>
        /// <param name="cancellationToken">رمز الإلغاء</param>
        /// <returns>المستخدم المطلوب أو null إذا لم يتم العثور عليه</returns>
        private async Task<AppUser?> FindUserAsync(string identifier, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    _logger.LogWarning("FindUserAsync: Empty identifier provided");
                    return null;
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    identifier = identifier.Trim();

                    // البحث بالبريد الإلكتروني
                    if (identifier.Contains("@"))
                    {
                        var emailUser = await _userManager.FindByEmailAsync(identifier);
                        if (emailUser != null)
                        {
                            _logger.LogDebug("User found by email: {Identifier}", identifier);
                            return emailUser;
                        }
                    }

                    // البحث برقم الهاتف (مع تحسين الأداء)
                    if (Regex.IsMatch(identifier, @"^\+?[\d\s\-]+$"))
                    {
                        var normalizedPhone = Regex.Replace(identifier, @"[^\d]", "");
                        var phoneUser = await _userManager.Users
                            //.AsNoTracking()
                            .FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone,
                                cancellationToken);

                        if (phoneUser != null)
                        {
                            _logger.LogDebug("User found by phone: {Identifier}", identifier);
                            return phoneUser;
                        }
                    }

                    // البحث باسم المستخدم
                    var usernameUser = await _userManager.FindByNameAsync(identifier);
                    if (usernameUser != null)
                    {
                        _logger.LogDebug("User found by username: {Identifier}", identifier);
                        return usernameUser;
                    }

                    _logger.LogInformation("User not found for identifier: {Identifier}", identifier);
                    return null;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("FindUserAsync operation was cancelled for identifier: {Identifier}", identifier);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error finding user for identifier: {Identifier}", identifier);
                    throw; // أو return null حسب متطلباتك
                }
            }

            private async Task<bool> SetLastLoginDate(AppUser user)
            {
                try
                {
                    if (user == null) return false;

                    user.LastLoginDate = DateTime.UtcNow;
                    var result = await _userManager.UpdateAsync(user);
                    return result.Succeeded;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error setting last login date for user {UserId}", user.Id);
                    return false;
                }
            }

            //public async Task<RequestResponse<LoginResponse>> UserPasswordSignInAsync(LoginRequest request)
            //{
            //    try
            //    {


            //        // التحقق من صحة المدخلات
            //        var validationResult = await _validationService.ValidateAsync(request);
            //        if (!validationResult.IsValid)
            //        {
            //            return RequestResponse<LoginResponse>.BadRequest(
            //                errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            //            );
            //        }

            //        var user = await FindUserAsync(request.UserNameOrEmailOrPhone,default);
            //        if (user == null)
            //            return RequestResponse<LoginResponse>.NotFound(_localizer["UserNotFound"]);

            //        if (user == null || !user.IsActive)
            //        {
            //            _logger.LogWarning("Login: Invalid attempt for user {Email}", request.UserNameOrEmailOrPhone);
            //            return RequestResponse<LoginResponse>.Unauthorized(_localizer["InvalidCredentials"]);
            //        }

            //        // التحقق من تأكيد البريد الإلكتروني
            //        //if (!user.EmailConfirmed)
            //        //    return RequestResponse<LoginResponse>.Unprocessable(_localizer["EmailNotConfirmed"]);

            //        var result = await _signInManager.CheckPasswordSignInAsync(
            //            user,
            //            request.Password,
            //            lockoutOnFailure: true // تمكين قفل الحساب بعد محاولات فاشلة
            //        );

            //        if (result.IsLockedOut)
            //            return RequestResponse<LoginResponse>.Locked(_localizer["AccountLocked"]);

            //        if (!result.Succeeded)
            //            return RequestResponse<LoginResponse>.Unauthorized(_localizer["InvalidCredentials"]);


            //        // توليد التوكن
            //        var roles = await _userManager.GetRolesAsync(user);
            //        var token = await _tokenService.GenerateJwtToken(user, roles.ToList()); // ✅

            //        return RequestResponse<LoginResponse>.Ok(
            //            data: new LoginResponse(token),
            //            message: _localizer["LoginSuccessful"]
            //        );
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Login error for email");
            //        return RequestResponse<LoginResponse>.InternalServerError(_localizer["SystemError"]);
            //    }
            //}


            private string MaskSensitiveExtensions(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;

                if (input.Contains("@")) // email
                {
                    var parts = input.Split('@');
                    if (parts[0].Length > 2)
                        return $"{parts[0].Substring(0, 2)}***@{parts[1]}";
                    return $"***@{parts[1]}";
                }

                if (Regex.IsMatch(input, @"^\+?\d+$")) // phone
                {
                    return input.Length > 4
                        ? $"{input.Substring(0, 2)}***{input.Substring(input.Length - 2)}"
                        : "***";
                }

                return input; // username - لا نقوم بإخفائه
            }
        //// دالة مساعدة للتحقق من صيغة البريد الإلكتروني
        //private bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var mailAddress = new System.Net.Mail.MailAddress(email);
        //        return mailAddress.Address == email;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        public async Task<RequestResponse<UserRegistrationResponse>> RegisterUser(Common.Contracts.DTO.Request.User.RegisterRequest dto, CancellationToken cancellationToken = default)
        {
            try
            {

                cancellationToken.ThrowIfCancellationRequested();

                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                {
                    return RequestResponse<UserRegistrationResponse>.BadRequest(_localizer["EmailAlreadyExists"]);
                }
                // التحقق من الصحة
                var validationResult = await _validationService.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return RequestResponse<UserRegistrationResponse>.BadRequest(
                        errors: validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    );
                }
                cancellationToken.ThrowIfCancellationRequested();

                // تحويل الـ DTO إلى كائن User باستخدام AutoMapper
                var user = _mapper.Map<AppUser>(dto);

                // إنشاء المستخدم
                var result = await _userManager.CreateAsync(user, dto.Password);
                cancellationToken.ThrowIfCancellationRequested();

                if (!result.Succeeded)
                {
                    return RequestResponse<UserRegistrationResponse>.Fail(
                        "Failed to create user",
                        result.Errors.Select(e => e.Description).ToList()
                    );
                }

                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync(DefaultRole))
                        await _userManager.AddToRoleAsync(user, DefaultRole);
                }
                return RequestResponse<UserRegistrationResponse>.Ok(
                    new UserRegistrationResponse(user.Id),
                    "User registered successfully"
                );
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Login operation was cancelled for identifier: {Identifier}", dto.FirstName);
                return RequestResponse<UserRegistrationResponse>.Fail("Operation was cancelled"); // 499 Client Closed Request
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
                return RequestResponse<UserRegistrationResponse>.InternalServerError();
            }
        }
        //    public async Task<RequestResponse<UserRegistrationResponse>> RegisterUser(Common.Contracts.DTO.Request.User.RegisterRequest dto, CancellationToken cancellationToken = default)
        //    {
        //        try
        //        {
        //            cancellationToken.ThrowIfCancellationRequested();

        //            // التحقق من البريد الإلكتروني
        //            if (await _userManager.FindByEmailAsync(dto.Email) != null)
        //            {
        //                return RequestResponse<UserRegistrationResponse>.Conflict(_localizer["EmailAlreadyExists"]);
        //            }

        //            // التحقق من الصحة
        //            var validationResult = await _validationService.ValidateAsync(dto);
        //            if (!validationResult.IsValid)
        //            {
        //                return RequestResponse<UserRegistrationResponse>.BadRequest(
        //                  errors: validationResult.Errors
        //.Select(e => _localizer[e.ErrorMessage].Value)
        //.ToList()
        //                );
        //            }

        //            cancellationToken.ThrowIfCancellationRequested();

        //            // تحويل الـ DTO إلى كائن User باستخدام AutoMapper
        //            var user = _mapper.Map<AppUser>(dto);

        //            // إنشاء المستخدم
        //            var result = await _userManager.CreateAsync(user, dto.Password);
        //            cancellationToken.ThrowIfCancellationRequested();

        //            if (!result.Succeeded)
        //            {
        //                return RequestResponse<UserRegistrationResponse>.Fail(
        //                    _localizer["FailedToCreateUser"],
        //                  errors: validationResult.Errors
        //.Select(e => _localizer[e.ErrorMessage].Value)
        //.ToList()
        //                );
        //            }

        //            // إضافة الدور الافتراضي
        //            if (result.Succeeded)
        //            {
        //                if (await _roleManager.RoleExistsAsync(DefaultRole))
        //                    await _userManager.AddToRoleAsync(user, DefaultRole);
        //            }

        //            return RequestResponse<UserRegistrationResponse>.Ok(
        //                new UserRegistrationResponse(user.Id),
        //                _localizer["UserRegisteredSuccessfully"]
        //            );
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            _logger.LogInformation("Login operation was cancelled for identifier: {Identifier}", dto.FirstName);
        //            return RequestResponse<UserRegistrationResponse>.Fail(_localizer["OperationCancelled"]); // 499 Client Closed Request
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
        //            return RequestResponse<UserRegistrationResponse>.InternalServerError();
        //        }
        //    }
        private async Task<Dictionary<string, List<string>>> GetUserRolesBatchAsync(List<string> userIds)
        {
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles[userId] = roles.ToList();
                }
            }

            return userRoles;
        }

        public async Task<RequestResponse<PagedResult<UserResponse>>> GetAllUsersAsync(
    UserQueryParameters queryParams,
    CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. التحقق من الإلغاء
                cancellationToken.ThrowIfCancellationRequested();

                // 2. التحقق من صحة المدخلات
                var validationResult = await _validationService.ValidateAsync(queryParams);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

                    _logger.LogWarning("GetAllUsers: Invalid query parameters - {Errors}",
                        string.Join(", ", errors));
                    return RequestResponse<PagedResult<UserResponse>>.BadRequest(
                        _localizer["InvalidQueryParameters"], errors);
                }

                // 3. بناء الاستعلام الأساسي مع التصفية المتقدمة
                var query = _userManager.Users.AsQueryable();

                query = ApplyFilters(query, queryParams);
                query = ApplySorting(query, queryParams.SortBy, queryParams.SortOrder);

                // 4. الحصول على العدد الكلي قبل التقسيم
                var totalCount = await query.CountAsync(cancellationToken);

                // 5. تطبيق التقسيم
                var users = await query
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync(cancellationToken);

                // 6. تحسين جلب الأدوار (Batch operation)
                var userRoles = await GetUserRolesBatchAsync(users.Select(u => u.Id).ToList());

                // 7. تعيين النتائج
                var userResponses = users.Select(user =>
                {
                    var response = _mapper.Map<UserResponse>(user);
                    response.Roles = userRoles.TryGetValue(user.Id, out var roles) ? roles : new List<string>();
                    return response;
                }).ToList();

                // 8. إنشاء النتيجة
                var result = new PagedResult<UserResponse>
                {
                    Items = userResponses,
                    TotalCount = totalCount,
                    PageNumber = queryParams.PageNumber,
                    PageSize = queryParams.PageSize
                };

                // 9. تسجيل النجاح
                _logger.LogInformation("Retrieved {Count} of {Total} users", userResponses.Count, totalCount);

                return RequestResponse<PagedResult<UserResponse>>.Ok(result, _localizer["UsersRetrievedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation was cancelled");
                return RequestResponse<PagedResult<UserResponse>>.Fail(_localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return RequestResponse<PagedResult<UserResponse>>.InternalServerError(_localizer["UnexpectedUsersError"]);
            }
        }
        private IQueryable<AppUser> ApplySorting(IQueryable<AppUser> query, string? sortBy, string? sortOrder)
        {
            sortOrder ??= "asc";
            sortBy ??= "UserName";

            return sortBy.ToLower() switch
            {
                "email" => sortOrder == "asc" ?
                    query.OrderBy(u => u.Email) :
                    query.OrderByDescending(u => u.Email),
                "firstname" => sortOrder == "asc" ?
                    query.OrderBy(u => u.FirstName) :
                    query.OrderByDescending(u => u.FirstName),
                "lastname" => sortOrder == "asc" ?
                    query.OrderBy(u => u.LastName) :
                    query.OrderByDescending(u => u.LastName),
                "createdat" => sortOrder == "asc" ?
                    query.OrderBy(u => u.CreatedAt) :
                    query.OrderByDescending(u => u.CreatedAt),
                "lastlogindate" => sortOrder == "asc" ?
                    query.OrderBy(u => u.LastLoginDate) :
                    query.OrderByDescending(u => u.LastLoginDate),
                _ => sortOrder == "asc" ?
                    query.OrderBy(u => u.UserName) :
                    query.OrderByDescending(u => u.UserName)
            };
        }

        // فئة معايير البحث

        // الدوال المساعدة
        private IQueryable<AppUser> ApplyFilters(IQueryable<AppUser> query, UserQueryParameters parameters)
        {
            if (query.Any())
            {
                if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
                {
                    var searchTerm = parameters.SearchTerm.Trim();
                    query = query.Where(u =>
                        u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.PhoneNumber.Contains(searchTerm));
                }

                if (parameters.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == parameters.IsActive);
                }

                if (parameters.Gender.HasValue && parameters.Gender != GenderType.Unknown)
                {
                    query = query.Where(u => u.Gender == parameters.Gender);
                }

                if (parameters.CreatedFrom.HasValue || parameters.CreatedTo.HasValue)
                {
                    if (parameters.CreatedFrom.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt >= parameters.CreatedFrom.Value);
                    }
                    if (parameters.CreatedTo.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt <= parameters.CreatedTo.Value);
                    }
                }

                if (parameters.LastLoginFrom.HasValue || parameters.LastLoginTo.HasValue)
                {
                    if (parameters.LastLoginFrom.HasValue)
                    {
                        query = query.Where(u => u.LastLoginDate >= parameters.LastLoginFrom.Value);
                    }
                    if (parameters.LastLoginTo.HasValue)
                    {
                        query = query.Where(u => u.LastLoginDate <= parameters.LastLoginTo.Value);
                    }
                }

                return query;

            }
            return query;


        }
        
        public async Task<RequestResponse> SoftDeleteUserAsync(
string userId,
CancellationToken cancellationToken = default)
        {
            // 1. التحقق من صحة المدخلات
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("DeleteUser: Attempted to delete user with empty ID");
                return RequestResponse.BadRequest(_localizer["UserCannotBeNull"]);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 2. البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("DeleteUser: User with ID {UserId} not found", userId);
                    return RequestResponse.NotFound(_localizer["UserNotFound"]);
                }

                // 3. تنفيذ الحذف الناعم (Soft Delete)
                user.IsActive = false;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors
                        .Select(e => _localizer[e.Description].ToString())
                        .ToList();

                    _logger.LogError(
                        "DeleteUser: Failed to soft delete user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", errors));

                    return RequestResponse.Fail(
                        _localizer["SoftDeleteUserFailed"],
                        errors);
                }

                // 4. إرسال حدث الحذف (اختياري)
                //await PublishDeactivationEventAsync(user.Id, cancellationToken);

                _logger.LogInformation(
                    "DeleteUser: Successfully soft deleted (deactivated) user {UserId}",
                    userId);

                return RequestResponse.Ok(_localizer["UserDeactivatedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteUser: Operation was cancelled for user {UserId}", userId);
                return RequestResponse.Fail(
                    _localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "DeleteUser: Unexpected error deactivating user {UserId}",
                    userId);

                return RequestResponse.InternalServerError(_localizer["UnexpectedError"]);
            }
        }
        public async Task<RequestResponse> RestorUserAsync(
string userId,
CancellationToken cancellationToken = default)
        {
            // 1. التحقق من صحة المدخلات
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("DeleteUser: Attempted to delete user with empty ID");
                return RequestResponse.BadRequest(_localizer["UserCannotBeNull"]);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 2. البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("DeleteUser: User with ID {UserId} not found", userId);
                    return RequestResponse.NotFound(_localizer["UserNotFound"]);
                }

                // 3. تنفيذ الحذف الناعم (Soft Delete)
                user.IsActive = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors
                        .Select(e => _localizer[e.Description].ToString())
                        .ToList();

                    _logger.LogError(
                        "DeleteUser: Failed to soft delete user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", errors));

                    return RequestResponse.Fail(
                        _localizer["SoftDeleteUserFailed"],
                        errors);
                }

                // 4. إرسال حدث الحذف (اختياري)
                //await PublishDeactivationEventAsync(user.Id, cancellationToken);

                _logger.LogInformation(
                    "DeleteUser: Successfully soft deleted (deactivated) user {UserId}",
                    userId);

                return RequestResponse.Ok(_localizer["UserDeactivatedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteUser: Operation was cancelled for user {UserId}", userId);
                return RequestResponse.Fail(
                    _localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "DeleteUser: Unexpected error deactivating user {UserId}",
                    userId);

                return RequestResponse.InternalServerError(_localizer["UnexpectedError"]);
            }
        }

        public async Task<RequestResponse> RemoveUserAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            // 1. التحقق من صحة المدخلات
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("RemoveUser: Attempted to delete user with empty ID");
                return RequestResponse.BadRequest(_localizer["UserCannotBeNull"]);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 2. البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("RemoveUser: User with ID {UserId} not found", userId);
                    return RequestResponse.NotFound(_localizer["UserNotFound"]);
                }

                // 3. حذف المستخدم
                var deleteResult = await _userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    var errors = deleteResult.Errors
                        .Select(e => _localizer[e.Description].ToString())
                        .ToList();

                    _logger.LogError(
                        "RemoveUser: Failed to delete user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", errors));

                    return RequestResponse.Fail(
                        _localizer["DeleteUserFailed"],
                        errors);
                }

                // 4. إرسال حدث الحذف
                //await PublishDeletionEventAsync(user.Id, cancellationToken);

                _logger.LogInformation(
                    "RemoveUser: Successfully deleted user {UserId}",
                    userId);

                return RequestResponse.Ok(_localizer["UserDeletedSuccessfully"]);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("RemoveUser: Operation was cancelled for user {UserId}", userId);
                return RequestResponse.Fail(
                    _localizer["OperationCancelled"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RemoveUser: Unexpected error deleting user {UserId}",
                    userId);

                return RequestResponse.InternalServerError(_localizer["UnexpectedError"]);
            }
        }
        private static string MapGenderToString(GenderType gender)
        {
            return gender switch
            {
                GenderType.Male => "Male",
                GenderType.Female => "Female",
                GenderType.Other => "Other",
                GenderType.PreferNotToSay => "Prefer not to say",
                _ => "Unknown"
            };
        }
        private bool HasUserChanges(UpdateUserRequest original, UpdateUserRequest updated)
        {
            if (original == null || updated == null) return false;

            return
                !string.Equals(original.FirstName, updated.FirstName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(original.LastName, updated.LastName, StringComparison.OrdinalIgnoreCase) ||
                original.DOB != updated.DOB ||
                !string.Equals(original.Phone, updated.Phone, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(original.Gender, updated.Gender, StringComparison.OrdinalIgnoreCase);
               
        }

        private bool HasUserChanges(UpdateUserRequest original, AppUser updated)
        {
            return !string.Equals(original.FirstName, updated.FirstName, StringComparison.Ordinal) ||
                   !string.Equals(original.LastName, updated.LastName, StringComparison.Ordinal) ||
                   !string.Equals(original.Phone, updated.PhoneNumber, StringComparison.Ordinal) ||
                   !string.Equals(original.Address, updated.Address, StringComparison.Ordinal) ||
                   original.DOB != updated.DOB ||
                   !string.Equals(original.Gender, MapGenderToString(updated.Gender), StringComparison.Ordinal) ||
                   !string.Equals(original.ProfilePictureUrl, updated.ProfilePictureUrl, StringComparison.Ordinal);
        }
        public async Task<RequestResponse<UserResponse>> UpdateUserAsync(
      string userId,
      UpdateUserRequest request,
      CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1. التحقق من صحة المدخلات
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("UpdateUser: Empty user ID provided");
                    return RequestResponse<UserResponse>.BadRequest(_localizer["UserIdRequired"]);
                }

                if (request is null)
                {
                    _logger.LogWarning("UpdateUser: Update request is null for user {UserId}", userId);
                    return RequestResponse<UserResponse>.BadRequest(_localizer["RequestCannotBeNull"]);
                }

                // 2. جلب المستخدم من قاعدة البيانات
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning("UpdateUser: User with ID {UserId} not found", userId);
                    return RequestResponse<UserResponse>.NotFound(_localizer["UserNotFound"]);
                }
                cancellationToken.ThrowIfCancellationRequested();

                // 3. التحقق من كلمة المرور إذا تم تقديمها
                if (!string.IsNullOrWhiteSpace(request.CurrentPassword) &&
                    !await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
                {
                    _logger.LogWarning("UpdateUser: Invalid password provided for user {UserId}", userId);
                    return RequestResponse<UserResponse>.Unauthorized(_localizer["InvalidPassword"]);
                }
                UpdateUserRequest originalUser = new UpdateUserRequest();
                originalUser.FirstName = user.FirstName;
                originalUser.LastName = user.LastName;

              

                // 5. تطبيق التحديثات على الكيان
                _mapper.Map(request, user);

                // 6. عمل نسخة بعد التحديث للمقارنة
                var updatedUser = _mapper.Map<UpdateUserRequest>(user);

                // 7. التحقق من وجود تغييرات فعلية
                if (!HasUserChanges(originalUser, updatedUser))
                {
                    _logger.LogInformation("UpdateUser: No changes detected for user {UserId}", userId);
                    return RequestResponse<UserResponse>.Error(
                        ResponseError.BadRequest,
                        _localizer["NoChangesDetected"]);
                }
                cancellationToken.ThrowIfCancellationRequested();

                // 8. التحقق من التكرارات (مثال: البريد الإلكتروني)
                //await ValidateUniqueConstraintsAsync(user, originalUser, cancellationToken);

                // 9. حفظ التغييرات
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors
                        .Select(e => _localizer[e.Description].ToString())
                        .ToList();

                    _logger.LogError(
                        "UpdateUser: Failed to update user {UserId}. Errors: {Errors}",
                        userId, string.Join(", ", errors));

                    return RequestResponse<UserResponse>.Fail(
                        _localizer["UpdateFailed"],
                        errors);
                }
                cancellationToken.ThrowIfCancellationRequested();

                // 10. إعداد الاستجابة

                var userResponse = _mapper.Map<UserResponse>(user);
                _logger.LogInformation(
                    "UpdateUser: User {UserId} updated successfully",
                    userId);

                return RequestResponse<UserResponse>.Ok(
                    userResponse,
                    _localizer["UpdateSuccessful"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "UpdateUser: Unexpected error while updating user {UserId}",
                    userId);

                return RequestResponse<UserResponse>.Error(
                    ResponseError.InternalServerError,
                    _localizer["UpdateError"]);
            }
        }


    }
}
