using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Common.Contracts.DTO.Request.User;
using Identity.API.Areas.Admin.ViewModel;
using Identity.Application.Contracts.DTO.Common;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Request.Users;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Enum;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Identity.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly ISharedMessageLocalizer _localizer;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IRoleService _roleService;
        public UserController(IUserService userService, ILogger<UserController> logger, ISharedMessageLocalizer localizer, UserManager<AppUser>userManager,RoleManager<ApplicationRole> roleManager,
            IRoleService roleService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleService = roleService;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
        }
        // GET: Admin/Users
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchTerm = "",
            bool? isActive = null,
            string gender = "",
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var parameters = new UserQueryParameters
                {
                    SearchTerm = searchTerm,
                    IsActive = isActive,
                    Gender = !string.IsNullOrEmpty(gender) ?
                           (GenderType)Enum.Parse(typeof(GenderType), gender) : null,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _userService.GetAllUsersAsync(parameters, cancellationToken);

                if (!result.IsSuccess)
                {
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<UserResponse>());
                }

                // حفظ معايير البحث في ViewBag لعرضها في الـ View
                ViewBag.SearchTerm = searchTerm;
                ViewBag.IsActive = isActive;
                ViewBag.Gender = gender;

                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحميل بيانات المستخدمين";
                return View(new PagedResult<UserResponse>());
            }
        }

        // GET: Admin/Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
         [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var result = await _userService.RegisterUser(request, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }

                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء إنشاء المستخدم";
                return View(request);
            }
        }

        //// GET: Admin/Users/Edit/5
        //public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            TempData["Error"] = "معرف المستخدم غير صالح";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var result = await _userManager.FindByIdAsync(id);

        //        if (result==null)
        //        {
        //            TempData["Error"] = result.Message;
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var editRequest = new UpdateUserRequest
        //        {

        //            FirstName = user.FirstName,
        //            LastName = user.LastName,
        //            UserName = user.UserName,
        //            Email = user.Email,
        //            PhoneNumber = user.PhoneNumber,
        //            Gender = user.Gender,
        //            IsActive = user.IsActive
        //        };

        //        return View(editRequest);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "حدث خطأ أثناء تحميل بيانات المستخدم";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        //// POST: Admin/Users/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(UpdateUserRequest request, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return View(request);
        //        }

        //        var result = await _userService.UpdateUserAsync(,request, cancellationToken);

        //        if (result.IsSuccess)
        //        {
        //            TempData["Success"] = result.Message;
        //            return RedirectToAction(nameof(Index));
        //        }

        //        foreach (var error in result.Errors)
        //        {
        //            ModelState.AddModelError(string.Empty, error);
        //        }

        //        return View(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "حدث خطأ أثناء تحديث المستخدم";
        //        return View(request);
        //    }
        //}

        // GET: Admin/Users/ManageRoles/5
        public async Task<IActionResult> ManageRoles(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "معرف المستخدم غير صالح";
                    return RedirectToAction(nameof(Index));
                }

                // جلب بيانات المستخدم
                var userResult = await _userManager.FindByIdAsync(userId);
                if (userResult == null)
                {
                    TempData["Error"] = "لم يتم العثور على المستخدم";
                    return RedirectToAction(nameof(Index));
                }

                // جلب جميع الأدوار
                var allRolesResult = await _roleService.GetAllRolesAsync(cancellationToken);
                if (!allRolesResult.IsSuccess)
                {
                    TempData["Error"] = "حدث خطأ أثناء جلب الأدوار";
                    return RedirectToAction(nameof(Index));
                }

                // جلب أدوار المستخدم الحالية
                var userRolesResult = await _roleService.GetUserRolesAsync(userId, cancellationToken);
                var currentUserRoles = userRolesResult.IsSuccess ? userRolesResult.Data.ToList() : new List<string>();

                var viewModel = new ManageUserRolesViewModel
                {
                    UserId = userId,
                    UserFullName = $"{userResult.FirstName} {userResult.LastName}",
                    UserEmail = userResult.Email,
                    UserRoles = currentUserRoles,
                    AllRoles = allRolesResult.Data.ToList(), // تم التصحيح هنا
                    SelectedRoles = currentUserRoles // تم التصحيح هنا
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحميل صفحة إدارة الأدوار للمستخدم: {UserId}", userId);
                TempData["Error"] = "حدث خطأ أثناء تحميل صفحة إدارة الأدوار";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Users/ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // إعادة تعبئة النموذج في حالة الخطأ
                    var allRolesResult = await _roleService.GetAllRolesAsync(cancellationToken);
                    model.AllRoles = allRolesResult.IsSuccess ? allRolesResult.Data.ToList() : new List<ApplicationRole>(); // تم التصحيح هنا
                    return View(model);
                }

                // إنشاء request object لدالة UpdateUserRolesAsync
                var updateRequest = new UpdateUserRolesRequest
                {
                    UserId = model.UserId,
                    RoleNames = model.SelectedRoles ?? new List<string>() // تم التصحيح هنا
                };

                var result = await _roleService.UpdateUserRolesAsync(updateRequest, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "تم تحديث أدوار المستخدم بنجاح";
                    return RedirectToAction(nameof(ManageRoles), new { userId = model.UserId });
                }

                // معالجة الأخطاء
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "حدث خطأ أثناء تحديث الأدوار");
                }

                // إعادة تعبئة النموذج في حالة الخطأ
                var allRoles = await _roleService.GetAllRolesAsync(cancellationToken);
                model.AllRoles = allRoles.IsSuccess ? allRoles.Data.ToList() : new List<ApplicationRole>(); // تم التصحيح هنا
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحديث أدوار المستخدم: {UserId}", model.UserId);
                TempData["Error"] = "حدث خطأ أثناء تحديث أدوار المستخدم";
                return RedirectToAction(nameof(ManageRoles), new { userId = model.UserId });
            }
        }
        // POST: Admin/Users/SoftDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "معرف المستخدم غير صالح";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userService.SoftDeleteUserAsync(userId, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تعطيل المستخدم";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Users/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "معرف المستخدم غير صالح";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userService.RestorUserAsync(userId, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء استعادة المستخدم";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "معرف المستخدم غير صالح";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userService.RemoveUserAsync(userId, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء حذف المستخدم";
                return RedirectToAction(nameof(Index));
            }
        }
          [HttpGet]
        public async Task<IActionResult> Edit(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = _localizer["UserIdRequired"];
                    return RedirectToAction(nameof(Index));
                }

                // جلب بيانات المستخدم الحالية
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = _localizer["UserNotFound"];
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UpdateUserRequestViewModel
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                
                   
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحميل صفحة تعديل المستخدم: {UserId}", id);
                TempData["Error"] = _localizer["LoadEditPageError"];
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserRequestViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // تحويل ViewModel إلى Request Model
                var updateRequest = new UpdateUserRequest
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CurrentPassword = model.CurrentPassword,
                 
                };

                // استدعاء خدمة التحديث
                var result = await _userService.UpdateUserAsync(model.UserId, updateRequest, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["Success"] = result.Message ?? _localizer["UpdateSuccessful"];
                    return RedirectToAction(nameof(Index));
                }

                // معالجة الأخطاء
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? _localizer["UpdateFailed"]);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحديث المستخدم: {UserId}", model.UserId);
                TempData["Error"] = _localizer["UpdateError"];
                return RedirectToAction(nameof(Edit), new { userId = model.UserId });
            }
        }

        // GET: Admin/Users/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = _localizer["UserIdRequired"];
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = _localizer["UserNotFound"];
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserResponseViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLoginDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ أثناء تحميل تفاصيل المستخدم: {UserId}", id);
                TempData["Error"] = _localizer["LoadDetailsError"];
                return RedirectToAction(nameof(Index));
            }
        }


        //// GET: Admin/Users/Details/5
        //public async Task<IActionResult> Details(string id, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            TempData["Error"] = "معرف المستخدم غير صالح";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var result = await _userService.GetUserByIdAsync(id, cancellationToken);

        //        if (!result.IsSuccess)
        //        {
        //            TempData["Error"] = result.Message;
        //            return RedirectToAction(nameof(Index));
        //        }

        //        // جلب أدوار المستخدم
        //        var rolesResult = await _userService.GetUserRolesAsync(id, cancellationToken);
        //        var roles = rolesResult.IsSuccess ? rolesResult.Data : new List<string>();

        //        var viewModel = new UserDetailsViewModel
        //        {
        //            User = result.Data,
        //            Roles = roles
        //        };

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل المستخدم";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        //// POST: Admin/Users/ChangePassword
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            TempData["Error"] = "بيانات كلمة المرور غير صالحة";
        //            return RedirectToAction(nameof(Edit), new { id = request.UserId });
        //        }

        //        var result = await _userService.ChangePasswordAsync(request, cancellationToken);

        //        if (result.IsSuccess)
        //        {
        //            TempData["Success"] = result.Message;
        //        }
        //        else
        //        {
        //            TempData["Error"] = result.Message;
        //        }

        //        return RedirectToAction(nameof(Edit), new { id = request.UserId });
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "حدث خطأ أثناء تغيير كلمة المرور";
        //        return RedirectToAction(nameof(Edit), new { id = request.UserId });
        //    }
        //}

        //    // AJAX Endpoint للتحقق من حالة المستخدم
        //    [HttpGet]
        //    public async Task<JsonResult> CheckUserStatus(string userId)
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(userId))
        //            {
        //                return Json(new { success = false, message = "معرف المستخدم غير صالح" });
        //            }

        //            var user = await _userManager.FindByIdAsync(userId);
        //            if (user == null)
        //            {
        //                return Json(new { success = false, message = "المستخدم غير موجود" });
        //            }

        //            return Json(new
        //            {
        //                success = true,
        //                isActive = user.IsActive,
        //                lastLogin = user.LastLoginDate?.ToString("yyyy/MM/dd HH:mm")
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { success = false, message = "حدث خطأ أثناء التحقق من حالة المستخدم" });
        //        }
        //    }
        //}
        private async Task SignInUserWithJwt(string jwtToken, string refreshToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            // استخراج الـ User Id من claim type "sub"
            var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Invalid token: missing sub claim");

            // استخراج الأدوار
            var roles = token.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            // استخراج الاسم والبريد الإلكتروني
            var userName = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            // ✅ إنشاء claims identity بشكل صحيح
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(ClaimTypes.Name, userName ?? userId),
        new Claim(ClaimTypes.Email, email ?? ""),
        new Claim("JWT_TOKEN", jwtToken)
    };

            // ✅ إضافة الأدوار بشكل صحيح
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // ✅ تسجيل الدخول باستخدام الإعدادات الصحيحة
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(2), // زيادة المدة
                AllowRefresh = true,
                IssuedUtc = DateTime.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ✅ إضافة الكوكيز
            Response.Cookies.Append("JWT_TOKEN", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax, // ✅ تغيير من Strict إلى Lax
                Expires = DateTime.UtcNow.AddHours(2),
                Path = "/"
            });

            Response.Cookies.Append("REFRESH_TOKEN", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(30),
                Path = "/"
            });
        }

        /// <summary>
        /// User login page
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginRequest(); // إنشاء instance جديد

            return View(model);
        }
        //private async Task SignInUserWithJwt(string jwtToken, string refreshToken)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var token = handler.ReadJwtToken(jwtToken);

        //    // استخراج الـ User Id من claim type "sub"
        //    var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        //    if (string.IsNullOrEmpty(userId))
        //        throw new Exception("Invalid token: missing sub claim");

        //    // استخراج الأدوار
        //    var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        //    // إنشاء claims identity مع تحديد scheme الكوكي
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, userId),
        //    new Claim(JwtRegisteredClaimNames.Sub, userId),
        //    new Claim("JWT_TOKEN", jwtToken) // إضافة التوكن كـ claim
        //};

        //    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        //    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //    // تسجيل الدخول باستخدام Cookie Authentication
        //    await HttpContext.SignInAsync(
        //        CookieAuthenticationDefaults.AuthenticationScheme,
        //        new ClaimsPrincipal(claimsIdentity),
        //        new AuthenticationProperties
        //        {
        //            IsPersistent = true,
        //            ExpiresUtc = DateTime.UtcNow.AddMinutes(15),
        //            AllowRefresh = true
        //        });

        //    // إضافة الكوكيز للـ JWT و Refresh Token
        //    Response.Cookies.Append("JWT_TOKEN", jwtToken, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.Strict,
        //        Expires = DateTime.UtcNow.AddMinutes(15)
        //    });

        //    Response.Cookies.Append("REFRESH_TOKEN", refreshToken, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        SameSite = SameSiteMode.Strict,
        //        Expires = DateTime.UtcNow.AddDays(30)
        //    });
        //}


        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    return View(request);
                }

                var result = await _userService.SignInAsync(request, cancellationToken);

                if (result.IsSuccess)
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(result.Data.AccessToken);

                    // جمع كل الأدوار الموجودة في الـ Claim
                    var roles = jwtToken.Claims
                       .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                       .Select(c => c.Value)
                       .ToList();


                    // التحقق من وجود الدور "Admin"
                    if (!roles.Contains("Admin"))
                    {
                        //TempData["ErrorMessage"] = "ليس لديك صلاحية للوصول إلى لوحة التحكم.";
                        await SignInUserWithJwt(result.Data.AccessToken, result.Data.RefreshToken);

                        return RedirectToAction("Index", "Home");
                    }
                    // Store token or user info in session/cookies if needed
                    await SignInUserWithJwt(result.Data.AccessToken, result.Data.RefreshToken);

                    return RedirectToAction("Index", "Admin");
                }

                // Handle different error cases using if-else instead of switch
                if (result.ErrorR == ResponseError.Unauthorized)
                {
                    ModelState.AddModelError("", "Invalid credentials");
                    TempData["ErrorMessage"] = result.Message;
                }
                else if (result.ErrorR == ResponseError.BadRequest)
                {
                    ModelState.AddModelError("", "Bad request");
                    TempData["ErrorMessage"] = result.Message;

                }
                else if (result.ErrorR == ResponseError.UnprocessableEntity)
                {
                    ModelState.AddModelError("", "Unprocessable entity");
                    TempData["ErrorMessage"] = result.Message;

                }
                else if (result.ErrorR == ResponseError.Locked)
                {
                    ModelState.AddModelError("", "Account locked");
                    TempData["ErrorMessage"] = result.Message;

                }
                else if (result.ErrorR == ResponseError.NotFound)
                {
                    ModelState.AddModelError("", "User not found");
                    TempData["ErrorMessage"] = result.Message;

                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred");
                    TempData["ErrorMessage"] = result.Message;

                }

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for : {Email}", request.UserNameOrEmailOrPhone);
                ModelState.AddModelError("", "An unexpected error occurred during login");
                return View(request);
            }
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// User sign-out endpoint
        /// </summary>
        [HttpPost("signout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _userService.SignOutAsync(cancellationToken);

                if (result.IsSuccess)
                {
                    // امسح الكوكيز الخاصة بالـ Cookie Authentication
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // امسح الكوكيز الخاصة بالـ JWT & RefreshToken
                    Response.Cookies.Delete("JWT_TOKEN");
                    Response.Cookies.Delete("REFRESH_TOKEN");

                    //TempData["SuccessMessage"] = "Signed out successfully!";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = result.Message ?? "Error during sign-out";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sign-out error for current user");
                TempData["ErrorMessage"] = "An unexpected error occurred during sign-out";
                return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// Refresh token endpoint (usually handled automatically in MVC)
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshtokenRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userService.RefreshTokenAsync(request, cancellationToken);

                if (!result.IsSuccess)
                {
                    TempData["ErrorMessage"] = "Token refresh failed";
                    return RedirectToAction("Login", "User");
                }

                // Update token in session/cookies if needed
                TempData["SuccessMessage"] = "Token refreshed successfully";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token error");
                TempData["ErrorMessage"] = "An unexpected error occurred during token refresh";
                return RedirectToAction("Login", "User");
            }
        }

        /// <summary>
        /// Register new user page
        /// </summary>
        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="dto">User registration data</param>
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterRequest dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                var result = await _userService.RegisterUser(dto, cancellationToken);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction("Login", "User");
                }

                // Handle different error cases using if-else
                if (result.ErrorR == ResponseError.BadRequest && result.Message == "EmailAlreadyExists")
                {
                    ModelState.AddModelError("Email", "Email already exists");
                }
                else if (result.ErrorR == ResponseError.BadRequest)
                {
                    ModelState.AddModelError("", "Bad request");
                }
                else if (result.ErrorR == ResponseError.UnprocessableEntity)
                {
                    ModelState.AddModelError("", "Unprocessable entity");
                }
                else
                {
                    ModelState.AddModelError("", "An unexpected error occurred during registration");
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for email: {Email}", dto.Email);
                ModelState.AddModelError("", "An unexpected error occurred during registration");
                return View(dto);
            }
        }

        ///// <summary>
        ///// Forgot password page
        ///// </summary>
        //[HttpGet("forgot-password")]
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// User profile page
        ///// </summary>
        //[HttpGet("profile")]
        //[Authorize]
        //public IActionResult Profile()
        //{
        //    return View();
        //}
    }
}
