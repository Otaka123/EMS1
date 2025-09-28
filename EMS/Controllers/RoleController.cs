using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Contracts.DTO.Request.Roles;
using Identity.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;
        private readonly ISharedMessageLocalizer _localizer;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleController(
            RoleManager<ApplicationRole> roleManager,
             UserManager<AppUser> userManager,
            IRoleService roleService,
            ILogger<RoleController> logger,
            ISharedMessageLocalizer localizer)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _roleManager = roleManager;
            _userManager = userManager;
        }
        ///// <summary>
        ///// Update user roles
        ///// </summary>
        ///// <param name="userId">User ID</param>
        ///// <param name="request">User roles update request</param>
        ///// <param name="cancellationToken">Cancellation token</param>
        ///// <returns>Update result</returns>
        //[HttpPut("users/{userId}/Updateroles")]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<RequestResponse>> UpdateUserRoles(
        //    [FromRoute] string userId,
        //    [FromBody] UpdateUserRolesRequest request,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
        //        }

        //        if (request == null)
        //        {
        //            return BadRequest(RequestResponse.BadRequest(_localizer["RequestCannotBeNull"]));
        //        }

        //        if (userId != request.UserId)
        //        {
        //            return BadRequest(RequestResponse.BadRequest(_localizer["UserIdMismatch"]));
        //        }

        //        var user = await _userManager.FindByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return NotFound(RequestResponse.NotFound(_localizer["UserNotFound"]));
        //        }

        //        var currentUserRoles = await _userManager.GetRolesAsync(user);

        //        var selectedRoles = new List<string>();
        //        foreach (var roleId in request.RoleNames)
        //        {
        //            var role = await _roleManager.FindByIdAsync(roleId);
        //            if (role != null)
        //            {
        //                selectedRoles.Add(role.Name);
        //            }
        //        }

        //        var rolesToAdd = selectedRoles.Except(currentUserRoles).ToList();
        //        var rolesToRemove = currentUserRoles.Except(selectedRoles).ToList();

        //        if (rolesToAdd.Any())
        //        {
        //            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
        //            if (!addResult.Succeeded)
        //            {
        //                var errors = addResult.Errors.Select(e => e.Description).ToList();
        //                return BadRequest(RequestResponse.Fail(_localizer["AddRolesFailed"], errors));
        //            }
        //        }

        //        if (rolesToRemove.Any())
        //        {
        //            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        //            if (!removeResult.Succeeded)
        //            {
        //                var errors = removeResult.Errors.Select(e => e.Description).ToList();
        //                return BadRequest(RequestResponse.Fail(_localizer["RemoveRolesFailed"], errors));
        //            }
        //        }

        //        return Ok(RequestResponse.Ok(_localizer["UserRolesUpdatedSuccessfully"]));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while updating roles for user: {UserId}", userId);
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            RequestResponse.InternalServerError(_localizer["UpdateUserRolesError"]));
        //    }
        //}

        /// <summary>
        /// Get all available roles
        /// </summary>
        [HttpGet("All")]
        [ProducesResponseType(typeof(RequestResponse<IEnumerable<IdentityRole>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<IEnumerable<ApplicationRole>>>> GetAllRoles(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.GetAllRolesAsync(cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }
        /// <summary>
        /// Get user roles management data
        /// </summary>
        [HttpGet("ManageRoles/{userId}")]
        [ProducesResponseType(typeof(RequestResponse<ManageUserRolesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<ManageUserRolesDTO>>> ManageRoles(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["InvalidUserId"]));
                }

                // جلب بيانات المستخدم
                var userResult = await _userManager.FindByIdAsync(userId);
                if (userResult == null)
                {
                    return NotFound(RequestResponse.NotFound(_localizer["UserNotFound"]));
                }

                // جلب جميع الأدوار
                var allRolesResult = await _roleService.GetAllRolesAsync(cancellationToken);
                if (!allRolesResult.IsSuccess)
                {
                    // استخدام HandleResult مع التحويل الصحيح
                    return HandleResult<ManageUserRolesDTO>(new RequestResponse<ManageUserRolesDTO>
                    {
                        IsSuccess = false,
                        Message = allRolesResult.Message,
                        ErrorR = allRolesResult.ErrorR
                    });
                }

                // جلب أدوار المستخدم الحالية
                var userRolesResult = await _roleService.GetUserRolesAsync(userId, cancellationToken);
                var currentUserRoles = userRolesResult.IsSuccess ? userRolesResult.Data.ToList() : new List<string>();

                var dto = new ManageUserRolesDTO
                {
                    UserId = userId,
                    UserFullName = $"{userResult.FirstName} {userResult.LastName}",
                    UserEmail = userResult.Email,
                    UserRoles = currentUserRoles,
                    SelectedRoles = currentUserRoles,
                    AllRoles = allRolesResult.Data.ToList() // هنا يجب أن يكون Data من نوع List<ApplicationRole>
                };

                return Ok(RequestResponse<ManageUserRolesDTO>.Success(dto, _localizer["RolesDataRetrieved"]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles data for user: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }
        /// <summary>
        /// Update user roles
        /// </summary>
        [HttpPost("ManageRoles")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> ManageUserRoles(
            [FromBody] ManageUserRolesDTO request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["InvalidData"]));
                }

                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(RequestResponse.NotFound(_localizer["UserNotFound"]));
                }

                // جلب أدوار المستخدم الحالية
                var currentUserRoles = await _userManager.GetRolesAsync(user);

                // جلب جميع الأدوار المحددة (Ids أو Names)
                var selectedRoles = new List<string>();
                foreach (var roleInput in request.SelectedRoles ?? new List<string>())
                {
                    IdentityRole? role = await _roleManager.FindByIdAsync(roleInput);

                    if (role == null)
                    {
                        // لو مش Id نجرب كـ Name
                        role = await _roleManager.FindByNameAsync(roleInput);
                    }

                    if (role != null)
                    {
                        selectedRoles.Add(role.Name);
                    }
                }

                // تحديد الأدوار المراد إضافتها
                var rolesToAdd = selectedRoles.Except(currentUserRoles).ToList();

                // تحديد الأدوار المراد إزالتها
                var rolesToRemove = currentUserRoles.Except(selectedRoles).ToList();

                // إضافة الأدوار الجديدة
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return BadRequest(RequestResponse.BadRequest($"فشل إضافة الأدوار: {errors}"));
                    }
                }

                // إزالة الأدوار غير المحددة
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        return BadRequest(RequestResponse.BadRequest($"فشل إزالة الأدوار: {errors}"));
                    }
                }

                return Ok(RequestResponse.Success(_localizer["RolesUpdatedSuccessfully"]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user: {UserId}", request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }


        /// <summary>
        /// Check if a role exists
        /// </summary>
        /// <param name="id">Role name to check</param>
        [HttpGet("Get/{id}")]
        [ProducesResponseType(typeof(RequestResponse<ApplicationRole>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<ApplicationRole>>> RoleExistsB(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.GetRoleByIdAsync(id, cancellationToken);
                
                return HandleResult(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleName}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }
        /// <summary>
        /// Check if a role exists
        /// </summary>
        /// <param name="roleName">Role name to check</param>
        [HttpGet("exists/{roleName}")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> RoleExists(string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.IsRoleExistsAsync(roleName, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleName}", roleName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost("Add")]
        [ProducesResponseType(typeof(RequestResponse<IdentityRole>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<ApplicationRole>>> CreateRole([FromBody] AddRoleRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.AddNewRoleAsync(request, cancellationToken);

                return result.IsSuccess switch
                {
                    true => CreatedAtAction(nameof(RoleExists), new { roleName = request.RoleName }, result),
                    false when result.ErrorR == ResponseError.Conflict => Conflict(result),
                    _ => HandleResult(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role: {RoleName}", request?.RoleName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        [HttpPut("Update")]
        [ProducesResponseType(typeof(RequestResponse<IdentityRole>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<ApplicationRole>>> UpdateRole([FromBody] EditRoleRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.EditRoleAsync(request, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role: {RoleId}", request?.RoleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="roleId">ID of the role to delete</param>
        [HttpDelete("{roleId}")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> DeleteRole(string roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(roleId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Assign role to user
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.AssignRoleToUserAsync(request, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}",
                    request?.RoleName, request?.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        /// <summary>
        /// Remove role from user
        /// </summary>
        [HttpPost("remove")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> RemoveRole([FromBody] RemoveRoleRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.RemoveUserFromRoleAsync(request, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}",
                    request?.RoleName, request?.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }
        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User roles information</returns>
        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<IEnumerable<string>>>> GetUserRoles(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var result = await _roleService.GetUserRolesAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["LoadUserRolesError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles for user: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["LoadUserRolesError"]));
            }
        }

        /// <summary>
        /// Update user roles
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="roleNames">List of role names</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}/UpdateUserRoles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> UpdateUserRoles(
            [FromRoute] string id,
            [FromBody] List<string> roleNames,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var updateRequest = new UpdateUserRolesRequest
                {
                    UserId = id,
                    RoleNames = roleNames ?? new List<string>()
                };

                var result = await _roleService.UpdateUserRolesAsync(updateRequest, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                        RequestResponse.InternalServerError(_localizer["UpdateUserRolesError"]))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating roles for user: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["UpdateUserRolesError"]));
            }
        }

        protected ActionResult<RequestResponse<T>> HandleResult<T>(RequestResponse<T> response)
        {
            if (response.IsSuccess)
            {
                return Ok(response); // إرجاع النتيجة مغلفة في RequestResponse
            }

            return response.ErrorR switch
            {
                ResponseError error => error.StatusCode switch
                {
                    400 => BadRequest(response),
                    401 => Unauthorized(response),
                    403 => Forbid(), // أو StatusCode(403, response)
                    404 => NotFound(response),
                    409 => Conflict(response),
                    422 => UnprocessableEntity(response),
                    _ => StatusCode(error.StatusCode, response)
                },
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
        //private ActionResult<T> HandleResult<T>(RequestResponse<T> result) where T : class
        //{
        //    return result.IsSuccess switch
        //    {
        //        true => Ok(result),
        //        false when result.ErrorR == ResponseError.NotFound => NotFound(result),
        //        false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
        //        false when result.ErrorR == ResponseError.Conflict => Conflict(result),
        //        false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
        //        _ => StatusCode(StatusCodes.Status500InternalServerError, result)
        //    };
        //}
    
        ///// <summary>
        ///// Bulk add permissions to role
        ///// </summary>
        //[HttpPost("{roleId}/permissions/bulk")]
        //[ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<RequestResponse<bool>>> BulkAddPermissionsToRole(
        //    string roleId,
        //    [FromBody] List<int> permissionIds,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        // تحتاج إلى تنفيذ هذه الدالة في الـ Service
        //        var result = await _roleService.BulkAddPermissionsToRoleAsync(roleId, permissionIds, cancellationToken);
        //        return HandleResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error bulk adding permissions to role {RoleId}", roleId);
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            RequestResponse.InternalServerError(_localizer["SystemError"]));
        //    }
        //}
        private ActionResult HandleResult(RequestResponse result)
        {
            return result.IsSuccess switch
            {
                true => Ok(result),
                false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                false when result.ErrorR == ResponseError.Conflict => Conflict(result),
                false when result.ErrorR == ResponseError.UnprocessableEntity => UnprocessableEntity(result),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }

    }
}
