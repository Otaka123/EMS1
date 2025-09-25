using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Request.Users;
using Identity.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Areas.Admin.Controllers
{
    //[Area("Admin")]

    //public class AdminController : Controller
    //{
    //    public IActionResult Index()
    //    {
    //        return View();
    //    }
    //}
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // تأكد أن فقط المشرفين يمكنهم الوصول
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IUserService userService, ILogger<AdminController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region إدارة المستخدمين - Views

        [HttpGet]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet]
        public IActionResult UserDetails(string id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        #endregion

        #region API Actions لإدارة المستخدمين

        /// <summary>
        /// جلب جميع المستخدمين مع Pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            int pageNumber = 1,
            int pageSize = 10,
            string searchTerm = "",
            bool? isActive = null,
            string sortBy = "CreatedAt",
            bool sortDescending = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var queryRequest = new UserQueryParameters
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    IsActive = isActive,
                    SortBy = sortBy,
                    
                };

                var result = await _userService.GetAllUsersAsync(queryRequest, cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        ///// <summary>
        ///// جلب مستخدم بواسطة ID
        ///// </summary>
        //[HttpGet]
        //public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return BadRequest(new { message = "User ID is required" });
        //        }

        //        var result = await _userService.GetUserByIdAsync(id, cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        if (result.StatusCode == 404)
        //        {
        //            return NotFound(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        ///// <summary>
        ///// جلب بيانات المستخدم الحالي
        ///// </summary>
        //[HttpGet]
        //public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var result = await _userService.GetCurrentUserAsync(cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        return Unauthorized(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving current user");
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        /// <summary>
        /// إنشاء مستخدم جديد
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                var result = await _userService.RegisterUser(request, cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        ///// <summary>
        ///// تحديث مستخدم
        ///// </summary>
        //[HttpPut]
        //public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { message = "Invalid data" });
        //        }

        //        // سيتم تنفيذ هذه الدالة في الـ UserService
        //        var result = await _userService.UpdateUserAsync(id, request, cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        if (result.StatusCode == 404)
        //        {
        //            return NotFound(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        /// <summary>
        /// حذف مستخدم (Soft Delete)
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { message = "User ID is required" });
                }

                // سيتم تنفيذ هذه الدالة في الـ UserService
                var result = await _userService.SoftDeleteUserAsync(id, cancellationToken);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                //if (result.StatusCode == 404)
                //{
                //    return NotFound(result);
                //}

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        ///// <summary>
        ///// تفعيل/تعطيل مستخدم
        ///// </summary>
        //[HttpPatch]
        //public async Task<IActionResult> ToggleUserStatus(string id, [FromBody] bool isActive, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return BadRequest(new { message = "User ID is required" });
        //        }

        //        // سيتم تنفيذ هذه الدالة في الـ UserService
        //        var result = await _userService.ToggleUserStatusAsync(id, isActive, cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        if (result.StatusCode == 404)
        //        {
        //            return NotFound(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error toggling status for user with ID: {UserId}", id);
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        ///// <summary>
        ///// إعادة تعيين كلمة المرور
        ///// </summary>
        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { message = "Invalid data" });
        //        }

        //        // سيتم تنفيذ هذه الدالة في الـ UserService
        //        var result = await _userService.ResetPasswordAsync(id, request, cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        if (result.StatusCode == 404)
        //        {
        //            return NotFound(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error resetting password for user with ID: {UserId}", id);
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        //#endregion

        //#region إحصائيات وتقارير

        ///// <summary>
        ///// جلب إحصائيات المستخدمين
        ///// </summary>
        //[HttpGet]
        //public async Task<IActionResult> GetUserStatistics(CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        // سيتم تنفيذ هذه الدالة في الـ UserService
        //        var result = await _userService.GetUserStatisticsAsync(cancellationToken);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }

        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving user statistics");
        //        return StatusCode(500, new { message = "Internal server error" });
        //    }
        //}

        #endregion
    }
}
