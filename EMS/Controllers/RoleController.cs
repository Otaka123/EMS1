using Common.Application.Common;
using Common.Application.Contracts.interfaces;
using Identity.Application.Contracts.DTO.Request.Roles;
using Identity.Application.Contracts.DTO.Response.Roles;
using Identity.Application.Contracts.Interfaces;
using Identity.Contracts.DTO.Request.Roles;
using Identity.Domain;
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

        public RoleController(
            IRoleService roleService,
            ILogger<RoleController> logger,
            ISharedMessageLocalizer localizer)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

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
        /// Get roles for a specific user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(RequestResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RequestResponse<IEnumerable<string>>>> GetUserRoles(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _roleService.GetUserRolesAsync(userId, cancellationToken);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    RequestResponse.InternalServerError(_localizer["SystemError"]));
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
