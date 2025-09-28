using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<PermissionController> _logger;
        private readonly ISharedMessageLocalizer _localizer;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public PermissionController(
             RoleManager<ApplicationRole> roleManager,
             UserManager<AppUser> userManager,
            IRoleService roleService,
            ILogger<PermissionController> logger,
            ISharedMessageLocalizer localizer)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }
        /// <summary>
        /// Get role permissions
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Role permissions information</returns>
        [HttpGet("{id}/permissions")]
        [ProducesResponseType(typeof(RequestResponse<RolePermissionsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<RolePermissionsResponse>>> GetRolePermissions(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["RoleIdRequired"]));
                }

                // التحقق من وجود الدور
                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(RequestResponse.NotFound(_localizer["RoleNotFound"]));
                }

                // جلب الصلاحيات الحالية للدور
                var currentPermissionsResult = await _roleService.GetPermissionsByRoleAsync(id, cancellationToken);
                if (!currentPermissionsResult.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, currentPermissionsResult);
                }

                // جلب جميع الصلاحيات المتاحة
                var allPermissionsResult = await _roleService.GetAllPermissionsAsync(cancellationToken);
                if (!allPermissionsResult.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, allPermissionsResult);
                }

                var response = new RolePermissionsResponse
                {
                    RoleId = id,
                    RoleName = role.Name,
                    CurrentPermissions = currentPermissionsResult.Data ?? new List<PermissionDTO>(),
                    AllPermissions = allPermissionsResult.Data ?? new List<PermissionDTO>()
                };

                return Ok(RequestResponse<RolePermissionsResponse>.Success(response, _localizer["PermissionsRetrievedSuccessfully"]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching permissions for role: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["LoadPermissionsError"]));
            }
        }
        [HttpGet("Allpermissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var result = await _roleService.GetAllPermissionsAsync();

                if (!result.IsSuccess)
                    return BadRequest(result.Message ?? "فشل في جلب الصلاحيات");

                return Ok(result.Data ?? new List<PermissionDTO>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"خطأ داخلي: {ex.Message}");
            }
        }

        /// <summary>
        /// Update role permissions
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="request">Permissions update request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}/permissions")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> UpdateRolePermissions(
            [FromRoute] string id,
            [FromBody] UpdateRolePermissionsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["RoleIdRequired"]));
                }

                if (request == null)
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["RequestCannotBeNull"]));
                }

                // التحقق من أن RoleId في الـ body يتطابق مع الـ route
                if (id != request.RoleId)
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["RoleIdMismatch"]));
                }

                var role = await _roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound(RequestResponse.NotFound(_localizer["RoleNotFound"]));
                }

                // جلب الصلاحيات الحالية للدور
                var currentPermissionsResult = await _roleService.GetPermissionsByRoleAsync(id, cancellationToken);
                if (!currentPermissionsResult.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, currentPermissionsResult);
                }

                var currentPermissionIds = currentPermissionsResult.Data?.Select(p => p.Id).ToList() ?? new List<int>();

                // تحديد الصلاحيات المراد إضافتها وإزالتها
                var permissionsToAdd = request.SelectedPermissionIds.Except(currentPermissionIds).ToList();
                var permissionsToRemove = currentPermissionIds.Except(request.SelectedPermissionIds).ToList();

                var errors = new List<string>();

                // إضافة الصلاحيات الجديدة
                foreach (var permissionId in permissionsToAdd)
                {
                    var result = await _roleService.AddPermissionToRoleAsync(id, permissionId, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        errors.Add($"Failed to add permission: {permissionId} - {result.Message}");
                    }
                }

                // إزالة الصلاحيات غير المحددة
                foreach (var permissionId in permissionsToRemove)
                {
                    var result = await _roleService.RemovePermissionFromRoleAsync(id, permissionId, cancellationToken);
                    if (!result.IsSuccess)
                    {
                        errors.Add($"Failed to remove permission: {permissionId} - {result.Message}");
                    }
                }

                if (errors.Any())
                {
                    return BadRequest(RequestResponse.Fail(_localizer["PartialUpdateSuccess"], errors));
                }

                return Ok(RequestResponse.Ok(_localizer["PermissionsUpdatedSuccessfully"]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating permissions for role: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["UpdatePermissionsError"]));
            }
        }

        /// <summary>
        /// Remove permission from role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="permissionId">Permission ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Remove result</returns>
        [HttpDelete("{id}/permissions/{permissionId}")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> RemovePermissionFromRole(
            [FromRoute] string id,
            [FromRoute] int permissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || permissionId <= 0)
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["InvalidParameters"]));
                }

                var result = await _roleService.RemovePermissionFromRoleAsync(id, permissionId, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing permission {PermissionId} from role {RoleId}", permissionId, id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["RemovePermissionError"]));
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse>> DeleteRole(
            [FromRoute] string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["RoleIdRequired"]));
                }

                var result = await _roleService.DeleteRoleAsync(id, cancellationToken);

                return result.IsSuccess switch
                {
                    true => Ok(result),
                    false when result.ErrorR == ResponseError.NotFound => NotFound(result),
                    false when result.ErrorR == ResponseError.BadRequest => BadRequest(result),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role: {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["DeleteRoleError"]));
            }
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User roles information</returns>
        [HttpGet("users/{userId}/roles")]
        [ProducesResponseType(typeof(RequestResponse<UserRolesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<UserRolesResponse>>> GetUserRoles(
            [FromRoute] string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(RequestResponse.BadRequest(_localizer["UserIdRequired"]));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(RequestResponse.NotFound(_localizer["UserNotFound"]));
                }

                var allRoles = await _roleManager.Roles.ToListAsync(cancellationToken);
                var userRoles = await _userManager.GetRolesAsync(user);

                var response = new UserRolesResponse
                {
                    UserId = userId,
                    UserName = user.UserName,
                    Email = user.Email,
                    CurrentRoles = userRoles.ToList(),
                    AllRoles = allRoles.Select(role => new UserRoleInfo
                    {
                        RoleId = role.Id,
                        RoleName = role.Name,
                        IsSelected = userRoles.Contains(role.Name)
                    }).ToList()
                };

                return Ok(RequestResponse<UserRolesResponse>.Success(response, _localizer["UserRolesRetrievedSuccessfully"]));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching roles for user: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["LoadUserRolesError"]));
            }
        }

      
        /// <summary>
        /// Add permission to role
        /// </summary>
        [HttpPost("{roleId}/AddPermissionToRole/{permissionId}")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> AddPermissionToRole(
            string roleId,
            int permissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.AddPermissionToRoleAsync(roleId, permissionId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding permission {PermissionId} to role {RoleId}", permissionId, roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        ///// <summary>
        ///// Remove permission from role
        ///// </summary>
        //[HttpDelete("{roleId}/RemovePermissionFromRole/{permissionId}")]
        //[ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<RequestResponse<bool>>> RemovePermissionFromRole(
        //    string roleId,
        //    int permissionId,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId, cancellationToken);
        //        return HandleResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, roleId);
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            RequestResponse.InternalServerError(_localizer["SystemError"]));
        //    }
        //}

        /// <summary>
        /// Update role permission (replace old permission with new one)
        /// </summary>
        [HttpPut("{roleId}/UpdateRolePermission")]
        [ProducesResponseType(typeof(RequestResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<bool>>> UpdateRolePermission(
            string roleId,
            [FromBody] UpdateRolePermissionRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.UpdateRolePermissionAsync(
                    roleId,
                    request.OldPermissionId,
                    request.NewPermissionId,
                    cancellationToken);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission for role {RoleId}", roleId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
            }
        }

        ///// <summary>
        ///// Get all permissions for a role
        ///// </summary>
        //[HttpGet("{roleId}/GetRolePermissions")]
        //[ProducesResponseType(typeof(RequestResponse<List<Permission>>), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult<RequestResponse<List<PermissionDTO>>>> GetRolePermissions(
        //    string roleId,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var result = await _roleService.GetPermissionsByRoleAsync(roleId, cancellationToken);
        //        return HandleResult(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting permissions for role {RoleId}", roleId);
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            RequestResponse.InternalServerError(_localizer["SystemError"]));
        //    }
        //}
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

    }
}
